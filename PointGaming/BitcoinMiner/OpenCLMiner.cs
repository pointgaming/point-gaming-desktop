using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCLNet;
using System.Runtime.InteropServices;
using System.Threading;

// some of these are needed by the OpenCL, but its not apparent
//using System.ComponentModel;
//using System.Diagnostics;
//using System.IO;
//using System.Xml;
//using System.Xml.Serialization;

namespace PointGaming.BitcoinMiner
{
    public class OpenCLMiner : Miner
    {
        public override string Name { get; protected set; }
        const int BlockLength = 16;
        const int StateLength = 8;
        const int TargetLength = 8;
        readonly uint workGroupSize = 0;

        readonly uint unit;
        uint globalThreads;


        public enum Vendor
        {
            Unknown,
            AMD,
            Intel,
            NVidia,
            
        }

        public static List<Miner> GetAvailableMiners() {
            List<Miner> available = new List<Miner>();

            DeviceType[] types = { DeviceType.GPU }; // DeviceType.CPU intel processor seems to make program unstable
            foreach (DeviceType type in types)
            {

                for (int pf = 0; pf < OpenCL.NumberOfPlatforms; pf++)
                {
                    try
                    {
                        int i = 0;

                        OpenCLManager OCLMan;
                        do
                        {
                            OCLMan = new OpenCLManager();
                            // Attempt to save binaries after compilation, as well as load precompiled binaries
                            // to avoid compilation. Usually you'll want this to be true. 
                            OCLMan.AttemptUseBinaries = true;
                            // Attempt to compile sources. This should probably be true for almost all projects.
                            // Setting it to false means that when you attempt to compile "mysource.cl", it will
                            // only scan the precompiled binary directory for a binary corresponding to a source
                            // with that name. There's a further restriction that the compiled binary also has to
                            // use the same Defines and BuildOptions
                            OCLMan.AttemptUseSource = true;
                            // Binary and source paths
                            // This is where we store our sources and where compiled binaries are placed
                            OCLMan.BinaryPath = @"BitcoinMiner\bin";
                            OCLMan.SourcePath = @"BitcoinMiner\src";
                            // If true, RequireImageSupport will filter out any devices without image support
                            // In this project we don't need image support though, so we set it to false
                            OCLMan.RequireImageSupport = false;
                            // The Defines string gets prepended to any and all sources that are compiled
                            // and serve as a convenient way to pass configuration information to the compilation process
                            OCLMan.Defines = "#define MyCompany_MyProject_Define 1";
                            // The BuildOptions string is passed directly to clBuild and can be used to do debug builds etc
                            OCLMan.BuildOptions = "";

                            OCLMan.CreateDefaultContext(pf, type);

                            string svendor = OCLMan.Context.Devices[i].Vendor;
                            Vendor vendor = Vendor.Unknown;

                            if (svendor == "NVIDIA Corporation")
                                vendor = Vendor.NVidia;
                            else if (svendor == "GenuineIntel")
                                vendor = Vendor.Intel;
                            else if (svendor == "Advanced Micro Devices, Inc.")
                                vendor = Vendor.AMD;
                            try
                            {
                                available.Add(new OpenCLMiner(OCLMan, vendor, i));
                            }
                            catch (Exception e)
                            {
                                //Util.LogException(e, "Miner");
                            }
                            i++;
                        }
                        while (i < OCLMan.Context.Devices.Length);
                    }
                    catch (Exception e)
                    {
                        //Util.LogException(e, "Miner");
                    }
                }
            }
            return available;
        }

        OpenCLManager OCLMan;
        //static OpenCLNet.Program programGeneral;
        //static OpenCLNet.Program programNVidia;
        //static Kernel searchGeneral;
        //static Kernel searchNVidia;
        Vendor vendor;

        public int deviceIndex = 0;
        public List<string> comboBoxDeviceSelector = new List<string>();

        public bool Initialized { get; private set; }

        public override Miner Copy() {
            return new OpenCLMiner(OCLMan, vendor, deviceIndex);
        }

        Kernel searchKernel;

        public OpenCLMiner(OpenCLManager OCLMan, Vendor vendor, int deviceIndex)
        {
            this.vendor = vendor;
            this.deviceIndex = deviceIndex;

            if (vendor == Vendor.NVidia)
                searchKernel = OCLMan.CompileFile("NVidia.cl")
                .CreateKernel("searchNVidia");
            else
                searchKernel = OCLMan.CompileFile("General.cl")
                .CreateKernel("searchGeneral");

            this.OCLMan = OCLMan;
            Device device = OCLMan.Context.Devices[deviceIndex];
            Name = device.Vendor + ":" + device.Name + " (" + deviceIndex + ")";

            unsafe
            {
                IntPtr size = (IntPtr)8;
                long[] values = new long[1];
                long[] sizeTest = new long[1];

                fixed (long* valuep = &values[0])
                {
                    IntPtr sizeOuts;

                    OpenCL.GetKernelWorkGroupInfo(searchKernel.KernelID, device.DeviceID, KernelWorkGroupInfo.WORK_GROUP_SIZE, size, (void*)valuep, out sizeOuts);
                    workGroupSize = (uint)values[0];
                }
            }
            
            unit = workGroupSize * 256u;
            globalThreads = (uint)(unit * 10);
            Initialized = true;
        }

        TimeSpan workTime;
        TimeSpan restTime;
        float UsageLimitLast = -1;
                
        protected override List<uint> ScanHash_CryptoPP(MinerData md, UInt256 target) {
            List<uint> results = new List<uint>();
            if (!Initialized) return results;

            var localhostUtc = DateTime.UtcNow;
            DateTime endLhutc = localhostUtc + new TimeSpan(0, 0, 1);
            long count = 0;
            long countEnd = uint.MaxValue - md.nHashesDone;

            DateTime startSearchLoop = localhostUtc;
            DateTime ticksEnd = startSearchLoop + new TimeSpan(0, 0, 10);
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            while (true)
            {
                watch.Start();
                if (globalThreads > 0)
                    results.AddRange(DoSearch(globalThreads, workGroupSize, md, target));
                watch.Stop();
                TimeSpan kt = watch.Elapsed;
                watch.Reset();
                float kernelTime = (float)kt.TotalSeconds;
                count += globalThreads;

                FPS = 1.0f / kernelTime;


                if (FPSLimit != 0)
                {
                    float lower = FPSLimit * 0.95f;
                    float upper = FPSLimit * 1.05f;
                    if (FPS > lower)
                        UM++;
                    else if (FPS < upper)
                        UM--;
                    if (UM < 1) UM = 1;
                    if (UM > UMLimit) UM = UMLimit;
                }
                else
                {
                    UM = UMLimit;
                }

                if (UsageLimitLast != UsageLimit)
                {
                    UsageLimitLast = UsageLimit;
                    restTime = TimeSpan.Zero;
                    workTime = TimeSpan.Zero;
                }
                else if (UsageLimit < 100)
                {
                    workTime += kt;
                    // workTime = (PercentUsage/100) * (workTime + restTime)
                    // workTime = (PercentUsage/100) * workTime + (PercentUsage/100) * restTime
                    // workTime - (PercentUsage/100) * workTime = (PercentUsage/100) * restTime
                    // restTime = (workTime - (PercentUsage/100) * workTime) / (PercentUsage/100)
                    float fUsage = UsageLimit / 100.0f;
                    if (fUsage <= 0) fUsage = 0.001f;
                    double sleepTime = ((1 - fUsage) * workTime.TotalMilliseconds) / fUsage - restTime.TotalMilliseconds;
                    watch.Start();
                    if (sleepTime >= 10.0) System.Threading.Thread.Sleep((int)sleepTime);
                    watch.Stop();
                    restTime += watch.Elapsed;
                    watch.Reset();
                }
                

                globalThreads = (uint)(unit * UM);

                if (count > countEnd || DateTime.UtcNow > endLhutc) break;
            }

            HashedSome(count);

            return results;
        }



        private List<uint> DoSearch(uint globalSize, uint localSize, MinerData md, UInt256 target)
        {
            List<uint> results = new List<uint>();

            uint[] output = new uint[2];

            uint target6 = target.getInt(6);

            unsafe
            {
                fixed (uint* outputp = &output[0])
                {
                    outputp[0] = 0;
                    outputp[1] = 1;
                    //outputp[2] = 0;

                    Mem outputBuffer = OCLMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR | MemFlags.WRITE_ONLY, sizeof(uint) * output.Length, outputp);

                    IntPtr[] CrossFadeGlobalWorkSize = new IntPtr[1];
                    CrossFadeGlobalWorkSize[0] = (IntPtr)globalSize;
                    IntPtr[] CrossFadeLocalWorkSize = new IntPtr[1];
                    CrossFadeLocalWorkSize[0] = (IntPtr)localSize;

                    byte[] tmp = new byte[4];
                    SetArgReverse(tmp, 0, 16, md.blockbuffer);
                    SetArgReverse(tmp, 1, 17, md.blockbuffer);
                    SetArgReverse(tmp, 2, 18, md.blockbuffer);
                    SetArg(tmp, 3, 0, md.midstatebuffer);
                    SetArg(tmp, 4, 1, md.midstatebuffer);
                    SetArg(tmp, 5, 2, md.midstatebuffer);
                    SetArg(tmp, 6, 3, md.midstatebuffer);
                    SetArg(tmp, 7, 4, md.midstatebuffer);
                    SetArg(tmp, 8, 5, md.midstatebuffer);
                    SetArg(tmp, 9, 6, md.midstatebuffer);
                    SetArg(tmp, 10, 7, md.midstatebuffer);

                    searchKernel.SetArg(11, target6);
                    searchKernel.SetArg(12, (uint)md.nHashesDone);
                    searchKernel.SetArg(13, outputBuffer);

                    //Event clEvent;
                    //Event event0 = OCLMan.Context.CreateUserEvent();
                    
                    CommandQueue CQ = OCLMan.CQ[deviceIndex];
                    CQ.EnqueueNDRangeKernel(searchKernel, 1, null, CrossFadeGlobalWorkSize, CrossFadeLocalWorkSize);
                    CQ.EnqueueBarrier();
                    CQ.EnqueueReadBuffer(outputBuffer, true, 0, (long)(sizeof(uint) * output.Length), (IntPtr)outputp);//, 0, null, out clEvent
                    //clEvent.SetCallback(ExecutionStatus.COMPLETE, TestUserEventCallback, this);
                    //CQ.EnqueueBarrier();
                    //areMineFinish.WaitOne();
                    CQ.Finish();
                    //OCLMan.Context.WaitForEvent(clEvent);
                    
                    if (output[0] == output[1]) results.Add(bytereverse(output[0]));

                    md.nHashesDone += globalSize;

                    outputBuffer.Dispose();
                }
            }
            
            return results;
        }

        unsafe private void SetArg(byte[] tmp, int argIndex, int bufferIndex, byte[] buffer)
        {
            Buffer.BlockCopy(buffer, bufferIndex * 4, tmp, 0, 4);
            uint arg2 = (uint)BitConverter.ToInt32(tmp, 0);
            searchKernel.SetArg(argIndex, arg2);
        }
        unsafe private void SetArgReverse(byte[] tmp, int argIndex, int bufferIndex, byte[] buffer)
        {
            Buffer.BlockCopy(buffer, bufferIndex * 4, tmp, 0, 4);
            uint arg2 = (uint)BitConverter.ToInt32(tmp, 0);
            arg2 = bytereverse(arg2);
            searchKernel.SetArg(argIndex, arg2);
        }

        private static uint bytereverse(uint x)
        {
            return (((x) << 24) | (((x) << 8) & 0x00ff0000) | (((x) >> 8) & 0x0000ff00) | ((x) >> 24));
        }

        //AutoResetEvent areMineFinish = new AutoResetEvent(false);

        //private void TestUserEventCallback(Event e, ExecutionStatus executionStatus, object userData)
        //{
        //    areMineFinish.Set();
        //}

    }
}
