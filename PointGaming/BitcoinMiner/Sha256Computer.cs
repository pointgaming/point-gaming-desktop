using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.BitcoinMiner
{
    public class Sha256Computer
    {
        public ulong _total;
        public readonly uint[] _state = new uint[8];
        public readonly byte[] _buffer = new byte[64];
        private readonly byte[] _myBuffer = new byte[64];
        private readonly byte[] _messageLength = new byte[8];

        public static void GET_UINT32(out uint n, byte[] b, int i)
        {
            (n) = ((uint)(b)[(i)] << 24)
                | ((uint)(b)[(i) + 1] << 16)
                | ((uint)(b)[(i) + 2] << 8)
                | ((uint)(b)[(i) + 3]);
        }

        public static void PUT_UINT32(uint n, byte[] b, int i)
        {
            (b)[(i)] = (byte)((n) >> 24);
            (b)[(i) + 1] = (byte)((n) >> 16);
            (b)[(i) + 2] = (byte)((n) >> 8);
            (b)[(i) + 3] = (byte)((n));
        }

        private static readonly byte[] sha256_padding =
        {
         0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        /// <summary>
        /// result is 32 bytes long
        /// </summary>
        public void Compute(byte[] input, byte[] result)
        {
            Init();
            Process(input, 0, (uint)input.Length);
            Finish(result);
        }

        /// <summary>
        /// result is 32 bytes long
        /// </summary>
        public void Compute(byte[] input, int offset, uint count, byte[] result)
        {
            Init();
            Process(input, offset, count);
            Finish(result);
        }

        public void Init()
        {
            _total = 0;

            var state = this._state;
            state[0] = 0x6A09E667;
            state[1] = 0xBB67AE85;
            state[2] = 0x3C6EF372;
            state[3] = 0xA54FF53A;
            state[4] = 0x510E527F;
            state[5] = 0x9B05688C;
            state[6] = 0x1F83D9AB;
            state[7] = 0x5BE0CD19;
        }

        public void Process(byte[] input)
        {
            Process(input, 0, (uint)input.Length);
        }

        public void Process(byte[] input, int offset, uint count)
        {
            int left, fill;

            if (count == 0) return;

            left = (int)(_total & 0x3F);
            fill = 64 - left;

            _total += count;
            
            var buffer = this._buffer;

            if (left != 0 && count >= fill)
            {
                Buffer.BlockCopy(input, offset, buffer, left, fill);
                MyProcess(buffer);
                count -= (uint)fill;
                offset += fill;
                left = 0;
            }

            var myBuffer = this._myBuffer;

            while (count >= 64)
            {
                Buffer.BlockCopy(input, offset, myBuffer, 0, 64);
                MyProcess(myBuffer);
                count -= 64;
                offset += 64;
            }

            if (count > 0)
                Buffer.BlockCopy(input, offset, buffer, left, (int)count);
        }

        /// <summary>
        /// result is 32 bytes long
        /// </summary>
        public void Finish(byte[] result)
        {
            uint last, padn;
            uint high, low;

            high = (uint)(_total >> 29);
            low = (uint)(_total << 3);

            var messageLength = this._messageLength;
            PUT_UINT32(high, messageLength, 0);
            PUT_UINT32(low, messageLength, 4);

            last = (uint)(_total & 0x3F);
            padn = (last < 56) ? (56 - last) : (120 - last);

            Process(sha256_padding, 0, padn);
            Process(messageLength, 0, 8);

            var state = this._state;
            PUT_UINT32(state[0], result, 0);
            PUT_UINT32(state[1], result, 4);
            PUT_UINT32(state[2], result, 8);
            PUT_UINT32(state[3], result, 12);
            PUT_UINT32(state[4], result, 16);
            PUT_UINT32(state[5], result, 20);
            PUT_UINT32(state[6], result, 24);
            PUT_UINT32(state[7], result, 28);
        }


        /// <summary>
        /// data must be 64 bytes long
        /// </summary>
        private void MyProcess(byte[] input)
        {
            uint temp1, temp2;
            uint A, B, C, D, E, F, G, H;

            uint w00, w01, w02, w03, w04, w05, w06, w07;
            uint w08, w09, w10, w11, w12, w13, w14, w15;
            uint w16, w17, w18, w19, w20, w21, w22, w23;
            uint w24, w25, w26, w27, w28, w29, w30, w31;
            uint w32, w33, w34, w35, w36, w37, w38, w39;
            uint w40, w41, w42, w43, w44, w45, w46, w47;
            uint w48, w49, w50, w51, w52, w53, w54, w55;
            uint w56, w57, w58, w59, w60, w61, w62, w63;

            w00 = (((uint)input[00]) << 24) | (((uint)input[01]) << 16) | (((uint)input[02]) << 8) | ((uint)input[03]);
            w01 = (((uint)input[04]) << 24) | (((uint)input[05]) << 16) | (((uint)input[06]) << 8) | ((uint)input[07]);
            w02 = (((uint)input[08]) << 24) | (((uint)input[09]) << 16) | (((uint)input[10]) << 8) | ((uint)input[11]);
            w03 = (((uint)input[12]) << 24) | (((uint)input[13]) << 16) | (((uint)input[14]) << 8) | ((uint)input[15]);
            w04 = (((uint)input[16]) << 24) | (((uint)input[17]) << 16) | (((uint)input[18]) << 8) | ((uint)input[19]);
            w05 = (((uint)input[20]) << 24) | (((uint)input[21]) << 16) | (((uint)input[22]) << 8) | ((uint)input[23]);
            w06 = (((uint)input[24]) << 24) | (((uint)input[25]) << 16) | (((uint)input[26]) << 8) | ((uint)input[27]);
            w07 = (((uint)input[28]) << 24) | (((uint)input[29]) << 16) | (((uint)input[30]) << 8) | ((uint)input[31]);
            w08 = (((uint)input[32]) << 24) | (((uint)input[33]) << 16) | (((uint)input[34]) << 8) | ((uint)input[35]);
            w09 = (((uint)input[36]) << 24) | (((uint)input[37]) << 16) | (((uint)input[38]) << 8) | ((uint)input[39]);
            w10 = (((uint)input[40]) << 24) | (((uint)input[41]) << 16) | (((uint)input[42]) << 8) | ((uint)input[43]);
            w11 = (((uint)input[44]) << 24) | (((uint)input[45]) << 16) | (((uint)input[46]) << 8) | ((uint)input[47]);
            w12 = (((uint)input[48]) << 24) | (((uint)input[49]) << 16) | (((uint)input[50]) << 8) | ((uint)input[51]);
            w13 = (((uint)input[52]) << 24) | (((uint)input[53]) << 16) | (((uint)input[54]) << 8) | ((uint)input[55]);
            w14 = (((uint)input[56]) << 24) | (((uint)input[57]) << 16) | (((uint)input[58]) << 8) | ((uint)input[59]);
            w15 = (((uint)input[60]) << 24) | (((uint)input[61]) << 16) | (((uint)input[62]) << 8) | ((uint)input[63]);

            var state = this._state;
            A = state[0];
            B = state[1];
            C = state[2];
            D = state[3];
            E = state[4];
            F = state[5];
            G = state[6];
            H = state[7];

            #region hash
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0x428A2F98 + w00; temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0x71374491 + w01; temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0xB5C0FBCF + w02; temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0xE9B5DBA5 + w03; temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x3956C25B + w04; temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0x59F111F1 + w05; temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x923F82A4 + w06; temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0xAB1C5ED5 + w07; temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0xD807AA98 + w08; temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0x12835B01 + w09; temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0x243185BE + w10; temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0x550C7DC3 + w11; temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x72BE5D74 + w12; temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0x80DEB1FE + w13; temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x9BDC06A7 + w14; temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0xC19BF174 + w15; temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0xE49B69C1 + (w16 = ((((w14 & 0xFFFFFFFF) >> 17) | (w14 << (32 - 17))) ^ (((w14 & 0xFFFFFFFF) >> 19) | (w14 << (32 - 19))) ^ ((w14 & 0xFFFFFFFF) >> 10)) + w09 + ((((w01 & 0xFFFFFFFF) >> 7) | (w01 << (32 - 7))) ^ (((w01 & 0xFFFFFFFF) >> 18) | (w01 << (32 - 18))) ^ ((w01 & 0xFFFFFFFF) >> 3)) + w00); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0xEFBE4786 + (w17 = ((((w15 & 0xFFFFFFFF) >> 17) | (w15 << (32 - 17))) ^ (((w15 & 0xFFFFFFFF) >> 19) | (w15 << (32 - 19))) ^ ((w15 & 0xFFFFFFFF) >> 10)) + w10 + ((((w02 & 0xFFFFFFFF) >> 7) | (w02 << (32 - 7))) ^ (((w02 & 0xFFFFFFFF) >> 18) | (w02 << (32 - 18))) ^ ((w02 & 0xFFFFFFFF) >> 3)) + w01); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0x0FC19DC6 + (w18 = ((((w16 & 0xFFFFFFFF) >> 17) | (w16 << (32 - 17))) ^ (((w16 & 0xFFFFFFFF) >> 19) | (w16 << (32 - 19))) ^ ((w16 & 0xFFFFFFFF) >> 10)) + w11 + ((((w03 & 0xFFFFFFFF) >> 7) | (w03 << (32 - 7))) ^ (((w03 & 0xFFFFFFFF) >> 18) | (w03 << (32 - 18))) ^ ((w03 & 0xFFFFFFFF) >> 3)) + w02); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0x240CA1CC + (w19 = ((((w17 & 0xFFFFFFFF) >> 17) | (w17 << (32 - 17))) ^ (((w17 & 0xFFFFFFFF) >> 19) | (w17 << (32 - 19))) ^ ((w17 & 0xFFFFFFFF) >> 10)) + w12 + ((((w04 & 0xFFFFFFFF) >> 7) | (w04 << (32 - 7))) ^ (((w04 & 0xFFFFFFFF) >> 18) | (w04 << (32 - 18))) ^ ((w04 & 0xFFFFFFFF) >> 3)) + w03); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x2DE92C6F + (w20 = ((((w18 & 0xFFFFFFFF) >> 17) | (w18 << (32 - 17))) ^ (((w18 & 0xFFFFFFFF) >> 19) | (w18 << (32 - 19))) ^ ((w18 & 0xFFFFFFFF) >> 10)) + w13 + ((((w05 & 0xFFFFFFFF) >> 7) | (w05 << (32 - 7))) ^ (((w05 & 0xFFFFFFFF) >> 18) | (w05 << (32 - 18))) ^ ((w05 & 0xFFFFFFFF) >> 3)) + w04); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0x4A7484AA + (w21 = ((((w19 & 0xFFFFFFFF) >> 17) | (w19 << (32 - 17))) ^ (((w19 & 0xFFFFFFFF) >> 19) | (w19 << (32 - 19))) ^ ((w19 & 0xFFFFFFFF) >> 10)) + w14 + ((((w06 & 0xFFFFFFFF) >> 7) | (w06 << (32 - 7))) ^ (((w06 & 0xFFFFFFFF) >> 18) | (w06 << (32 - 18))) ^ ((w06 & 0xFFFFFFFF) >> 3)) + w05); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x5CB0A9DC + (w22 = ((((w20 & 0xFFFFFFFF) >> 17) | (w20 << (32 - 17))) ^ (((w20 & 0xFFFFFFFF) >> 19) | (w20 << (32 - 19))) ^ ((w20 & 0xFFFFFFFF) >> 10)) + w15 + ((((w07 & 0xFFFFFFFF) >> 7) | (w07 << (32 - 7))) ^ (((w07 & 0xFFFFFFFF) >> 18) | (w07 << (32 - 18))) ^ ((w07 & 0xFFFFFFFF) >> 3)) + w06); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0x76F988DA + (w23 = ((((w21 & 0xFFFFFFFF) >> 17) | (w21 << (32 - 17))) ^ (((w21 & 0xFFFFFFFF) >> 19) | (w21 << (32 - 19))) ^ ((w21 & 0xFFFFFFFF) >> 10)) + w16 + ((((w08 & 0xFFFFFFFF) >> 7) | (w08 << (32 - 7))) ^ (((w08 & 0xFFFFFFFF) >> 18) | (w08 << (32 - 18))) ^ ((w08 & 0xFFFFFFFF) >> 3)) + w07); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0x983E5152 + (w24 = ((((w22 & 0xFFFFFFFF) >> 17) | (w22 << (32 - 17))) ^ (((w22 & 0xFFFFFFFF) >> 19) | (w22 << (32 - 19))) ^ ((w22 & 0xFFFFFFFF) >> 10)) + w17 + ((((w09 & 0xFFFFFFFF) >> 7) | (w09 << (32 - 7))) ^ (((w09 & 0xFFFFFFFF) >> 18) | (w09 << (32 - 18))) ^ ((w09 & 0xFFFFFFFF) >> 3)) + w08); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0xA831C66D + (w25 = ((((w23 & 0xFFFFFFFF) >> 17) | (w23 << (32 - 17))) ^ (((w23 & 0xFFFFFFFF) >> 19) | (w23 << (32 - 19))) ^ ((w23 & 0xFFFFFFFF) >> 10)) + w18 + ((((w10 & 0xFFFFFFFF) >> 7) | (w10 << (32 - 7))) ^ (((w10 & 0xFFFFFFFF) >> 18) | (w10 << (32 - 18))) ^ ((w10 & 0xFFFFFFFF) >> 3)) + w09); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0xB00327C8 + (w26 = ((((w24 & 0xFFFFFFFF) >> 17) | (w24 << (32 - 17))) ^ (((w24 & 0xFFFFFFFF) >> 19) | (w24 << (32 - 19))) ^ ((w24 & 0xFFFFFFFF) >> 10)) + w19 + ((((w11 & 0xFFFFFFFF) >> 7) | (w11 << (32 - 7))) ^ (((w11 & 0xFFFFFFFF) >> 18) | (w11 << (32 - 18))) ^ ((w11 & 0xFFFFFFFF) >> 3)) + w10); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0xBF597FC7 + (w27 = ((((w25 & 0xFFFFFFFF) >> 17) | (w25 << (32 - 17))) ^ (((w25 & 0xFFFFFFFF) >> 19) | (w25 << (32 - 19))) ^ ((w25 & 0xFFFFFFFF) >> 10)) + w20 + ((((w12 & 0xFFFFFFFF) >> 7) | (w12 << (32 - 7))) ^ (((w12 & 0xFFFFFFFF) >> 18) | (w12 << (32 - 18))) ^ ((w12 & 0xFFFFFFFF) >> 3)) + w11); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0xC6E00BF3 + (w28 = ((((w26 & 0xFFFFFFFF) >> 17) | (w26 << (32 - 17))) ^ (((w26 & 0xFFFFFFFF) >> 19) | (w26 << (32 - 19))) ^ ((w26 & 0xFFFFFFFF) >> 10)) + w21 + ((((w13 & 0xFFFFFFFF) >> 7) | (w13 << (32 - 7))) ^ (((w13 & 0xFFFFFFFF) >> 18) | (w13 << (32 - 18))) ^ ((w13 & 0xFFFFFFFF) >> 3)) + w12); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0xD5A79147 + (w29 = ((((w27 & 0xFFFFFFFF) >> 17) | (w27 << (32 - 17))) ^ (((w27 & 0xFFFFFFFF) >> 19) | (w27 << (32 - 19))) ^ ((w27 & 0xFFFFFFFF) >> 10)) + w22 + ((((w14 & 0xFFFFFFFF) >> 7) | (w14 << (32 - 7))) ^ (((w14 & 0xFFFFFFFF) >> 18) | (w14 << (32 - 18))) ^ ((w14 & 0xFFFFFFFF) >> 3)) + w13); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x06CA6351 + (w30 = ((((w28 & 0xFFFFFFFF) >> 17) | (w28 << (32 - 17))) ^ (((w28 & 0xFFFFFFFF) >> 19) | (w28 << (32 - 19))) ^ ((w28 & 0xFFFFFFFF) >> 10)) + w23 + ((((w15 & 0xFFFFFFFF) >> 7) | (w15 << (32 - 7))) ^ (((w15 & 0xFFFFFFFF) >> 18) | (w15 << (32 - 18))) ^ ((w15 & 0xFFFFFFFF) >> 3)) + w14); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0x14292967 + (w31 = ((((w29 & 0xFFFFFFFF) >> 17) | (w29 << (32 - 17))) ^ (((w29 & 0xFFFFFFFF) >> 19) | (w29 << (32 - 19))) ^ ((w29 & 0xFFFFFFFF) >> 10)) + w24 + ((((w16 & 0xFFFFFFFF) >> 7) | (w16 << (32 - 7))) ^ (((w16 & 0xFFFFFFFF) >> 18) | (w16 << (32 - 18))) ^ ((w16 & 0xFFFFFFFF) >> 3)) + w15); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0x27B70A85 + (w32 = ((((w30 & 0xFFFFFFFF) >> 17) | (w30 << (32 - 17))) ^ (((w30 & 0xFFFFFFFF) >> 19) | (w30 << (32 - 19))) ^ ((w30 & 0xFFFFFFFF) >> 10)) + w25 + ((((w17 & 0xFFFFFFFF) >> 7) | (w17 << (32 - 7))) ^ (((w17 & 0xFFFFFFFF) >> 18) | (w17 << (32 - 18))) ^ ((w17 & 0xFFFFFFFF) >> 3)) + w16); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0x2E1B2138 + (w33 = ((((w31 & 0xFFFFFFFF) >> 17) | (w31 << (32 - 17))) ^ (((w31 & 0xFFFFFFFF) >> 19) | (w31 << (32 - 19))) ^ ((w31 & 0xFFFFFFFF) >> 10)) + w26 + ((((w18 & 0xFFFFFFFF) >> 7) | (w18 << (32 - 7))) ^ (((w18 & 0xFFFFFFFF) >> 18) | (w18 << (32 - 18))) ^ ((w18 & 0xFFFFFFFF) >> 3)) + w17); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0x4D2C6DFC + (w34 = ((((w32 & 0xFFFFFFFF) >> 17) | (w32 << (32 - 17))) ^ (((w32 & 0xFFFFFFFF) >> 19) | (w32 << (32 - 19))) ^ ((w32 & 0xFFFFFFFF) >> 10)) + w27 + ((((w19 & 0xFFFFFFFF) >> 7) | (w19 << (32 - 7))) ^ (((w19 & 0xFFFFFFFF) >> 18) | (w19 << (32 - 18))) ^ ((w19 & 0xFFFFFFFF) >> 3)) + w18); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0x53380D13 + (w35 = ((((w33 & 0xFFFFFFFF) >> 17) | (w33 << (32 - 17))) ^ (((w33 & 0xFFFFFFFF) >> 19) | (w33 << (32 - 19))) ^ ((w33 & 0xFFFFFFFF) >> 10)) + w28 + ((((w20 & 0xFFFFFFFF) >> 7) | (w20 << (32 - 7))) ^ (((w20 & 0xFFFFFFFF) >> 18) | (w20 << (32 - 18))) ^ ((w20 & 0xFFFFFFFF) >> 3)) + w19); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x650A7354 + (w36 = ((((w34 & 0xFFFFFFFF) >> 17) | (w34 << (32 - 17))) ^ (((w34 & 0xFFFFFFFF) >> 19) | (w34 << (32 - 19))) ^ ((w34 & 0xFFFFFFFF) >> 10)) + w29 + ((((w21 & 0xFFFFFFFF) >> 7) | (w21 << (32 - 7))) ^ (((w21 & 0xFFFFFFFF) >> 18) | (w21 << (32 - 18))) ^ ((w21 & 0xFFFFFFFF) >> 3)) + w20); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0x766A0ABB + (w37 = ((((w35 & 0xFFFFFFFF) >> 17) | (w35 << (32 - 17))) ^ (((w35 & 0xFFFFFFFF) >> 19) | (w35 << (32 - 19))) ^ ((w35 & 0xFFFFFFFF) >> 10)) + w30 + ((((w22 & 0xFFFFFFFF) >> 7) | (w22 << (32 - 7))) ^ (((w22 & 0xFFFFFFFF) >> 18) | (w22 << (32 - 18))) ^ ((w22 & 0xFFFFFFFF) >> 3)) + w21); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x81C2C92E + (w38 = ((((w36 & 0xFFFFFFFF) >> 17) | (w36 << (32 - 17))) ^ (((w36 & 0xFFFFFFFF) >> 19) | (w36 << (32 - 19))) ^ ((w36 & 0xFFFFFFFF) >> 10)) + w31 + ((((w23 & 0xFFFFFFFF) >> 7) | (w23 << (32 - 7))) ^ (((w23 & 0xFFFFFFFF) >> 18) | (w23 << (32 - 18))) ^ ((w23 & 0xFFFFFFFF) >> 3)) + w22); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0x92722C85 + (w39 = ((((w37 & 0xFFFFFFFF) >> 17) | (w37 << (32 - 17))) ^ (((w37 & 0xFFFFFFFF) >> 19) | (w37 << (32 - 19))) ^ ((w37 & 0xFFFFFFFF) >> 10)) + w32 + ((((w24 & 0xFFFFFFFF) >> 7) | (w24 << (32 - 7))) ^ (((w24 & 0xFFFFFFFF) >> 18) | (w24 << (32 - 18))) ^ ((w24 & 0xFFFFFFFF) >> 3)) + w23); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0xA2BFE8A1 + (w40 = ((((w38 & 0xFFFFFFFF) >> 17) | (w38 << (32 - 17))) ^ (((w38 & 0xFFFFFFFF) >> 19) | (w38 << (32 - 19))) ^ ((w38 & 0xFFFFFFFF) >> 10)) + w33 + ((((w25 & 0xFFFFFFFF) >> 7) | (w25 << (32 - 7))) ^ (((w25 & 0xFFFFFFFF) >> 18) | (w25 << (32 - 18))) ^ ((w25 & 0xFFFFFFFF) >> 3)) + w24); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0xA81A664B + (w41 = ((((w39 & 0xFFFFFFFF) >> 17) | (w39 << (32 - 17))) ^ (((w39 & 0xFFFFFFFF) >> 19) | (w39 << (32 - 19))) ^ ((w39 & 0xFFFFFFFF) >> 10)) + w34 + ((((w26 & 0xFFFFFFFF) >> 7) | (w26 << (32 - 7))) ^ (((w26 & 0xFFFFFFFF) >> 18) | (w26 << (32 - 18))) ^ ((w26 & 0xFFFFFFFF) >> 3)) + w25); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0xC24B8B70 + (w42 = ((((w40 & 0xFFFFFFFF) >> 17) | (w40 << (32 - 17))) ^ (((w40 & 0xFFFFFFFF) >> 19) | (w40 << (32 - 19))) ^ ((w40 & 0xFFFFFFFF) >> 10)) + w35 + ((((w27 & 0xFFFFFFFF) >> 7) | (w27 << (32 - 7))) ^ (((w27 & 0xFFFFFFFF) >> 18) | (w27 << (32 - 18))) ^ ((w27 & 0xFFFFFFFF) >> 3)) + w26); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0xC76C51A3 + (w43 = ((((w41 & 0xFFFFFFFF) >> 17) | (w41 << (32 - 17))) ^ (((w41 & 0xFFFFFFFF) >> 19) | (w41 << (32 - 19))) ^ ((w41 & 0xFFFFFFFF) >> 10)) + w36 + ((((w28 & 0xFFFFFFFF) >> 7) | (w28 << (32 - 7))) ^ (((w28 & 0xFFFFFFFF) >> 18) | (w28 << (32 - 18))) ^ ((w28 & 0xFFFFFFFF) >> 3)) + w27); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0xD192E819 + (w44 = ((((w42 & 0xFFFFFFFF) >> 17) | (w42 << (32 - 17))) ^ (((w42 & 0xFFFFFFFF) >> 19) | (w42 << (32 - 19))) ^ ((w42 & 0xFFFFFFFF) >> 10)) + w37 + ((((w29 & 0xFFFFFFFF) >> 7) | (w29 << (32 - 7))) ^ (((w29 & 0xFFFFFFFF) >> 18) | (w29 << (32 - 18))) ^ ((w29 & 0xFFFFFFFF) >> 3)) + w28); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0xD6990624 + (w45 = ((((w43 & 0xFFFFFFFF) >> 17) | (w43 << (32 - 17))) ^ (((w43 & 0xFFFFFFFF) >> 19) | (w43 << (32 - 19))) ^ ((w43 & 0xFFFFFFFF) >> 10)) + w38 + ((((w30 & 0xFFFFFFFF) >> 7) | (w30 << (32 - 7))) ^ (((w30 & 0xFFFFFFFF) >> 18) | (w30 << (32 - 18))) ^ ((w30 & 0xFFFFFFFF) >> 3)) + w29); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0xF40E3585 + (w46 = ((((w44 & 0xFFFFFFFF) >> 17) | (w44 << (32 - 17))) ^ (((w44 & 0xFFFFFFFF) >> 19) | (w44 << (32 - 19))) ^ ((w44 & 0xFFFFFFFF) >> 10)) + w39 + ((((w31 & 0xFFFFFFFF) >> 7) | (w31 << (32 - 7))) ^ (((w31 & 0xFFFFFFFF) >> 18) | (w31 << (32 - 18))) ^ ((w31 & 0xFFFFFFFF) >> 3)) + w30); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0x106AA070 + (w47 = ((((w45 & 0xFFFFFFFF) >> 17) | (w45 << (32 - 17))) ^ (((w45 & 0xFFFFFFFF) >> 19) | (w45 << (32 - 19))) ^ ((w45 & 0xFFFFFFFF) >> 10)) + w40 + ((((w32 & 0xFFFFFFFF) >> 7) | (w32 << (32 - 7))) ^ (((w32 & 0xFFFFFFFF) >> 18) | (w32 << (32 - 18))) ^ ((w32 & 0xFFFFFFFF) >> 3)) + w31); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0x19A4C116 + (w48 = ((((w46 & 0xFFFFFFFF) >> 17) | (w46 << (32 - 17))) ^ (((w46 & 0xFFFFFFFF) >> 19) | (w46 << (32 - 19))) ^ ((w46 & 0xFFFFFFFF) >> 10)) + w41 + ((((w33 & 0xFFFFFFFF) >> 7) | (w33 << (32 - 7))) ^ (((w33 & 0xFFFFFFFF) >> 18) | (w33 << (32 - 18))) ^ ((w33 & 0xFFFFFFFF) >> 3)) + w32); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0x1E376C08 + (w49 = ((((w47 & 0xFFFFFFFF) >> 17) | (w47 << (32 - 17))) ^ (((w47 & 0xFFFFFFFF) >> 19) | (w47 << (32 - 19))) ^ ((w47 & 0xFFFFFFFF) >> 10)) + w42 + ((((w34 & 0xFFFFFFFF) >> 7) | (w34 << (32 - 7))) ^ (((w34 & 0xFFFFFFFF) >> 18) | (w34 << (32 - 18))) ^ ((w34 & 0xFFFFFFFF) >> 3)) + w33); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0x2748774C + (w50 = ((((w48 & 0xFFFFFFFF) >> 17) | (w48 << (32 - 17))) ^ (((w48 & 0xFFFFFFFF) >> 19) | (w48 << (32 - 19))) ^ ((w48 & 0xFFFFFFFF) >> 10)) + w43 + ((((w35 & 0xFFFFFFFF) >> 7) | (w35 << (32 - 7))) ^ (((w35 & 0xFFFFFFFF) >> 18) | (w35 << (32 - 18))) ^ ((w35 & 0xFFFFFFFF) >> 3)) + w34); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0x34B0BCB5 + (w51 = ((((w49 & 0xFFFFFFFF) >> 17) | (w49 << (32 - 17))) ^ (((w49 & 0xFFFFFFFF) >> 19) | (w49 << (32 - 19))) ^ ((w49 & 0xFFFFFFFF) >> 10)) + w44 + ((((w36 & 0xFFFFFFFF) >> 7) | (w36 << (32 - 7))) ^ (((w36 & 0xFFFFFFFF) >> 18) | (w36 << (32 - 18))) ^ ((w36 & 0xFFFFFFFF) >> 3)) + w35); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x391C0CB3 + (w52 = ((((w50 & 0xFFFFFFFF) >> 17) | (w50 << (32 - 17))) ^ (((w50 & 0xFFFFFFFF) >> 19) | (w50 << (32 - 19))) ^ ((w50 & 0xFFFFFFFF) >> 10)) + w45 + ((((w37 & 0xFFFFFFFF) >> 7) | (w37 << (32 - 7))) ^ (((w37 & 0xFFFFFFFF) >> 18) | (w37 << (32 - 18))) ^ ((w37 & 0xFFFFFFFF) >> 3)) + w36); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0x4ED8AA4A + (w53 = ((((w51 & 0xFFFFFFFF) >> 17) | (w51 << (32 - 17))) ^ (((w51 & 0xFFFFFFFF) >> 19) | (w51 << (32 - 19))) ^ ((w51 & 0xFFFFFFFF) >> 10)) + w46 + ((((w38 & 0xFFFFFFFF) >> 7) | (w38 << (32 - 7))) ^ (((w38 & 0xFFFFFFFF) >> 18) | (w38 << (32 - 18))) ^ ((w38 & 0xFFFFFFFF) >> 3)) + w37); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0x5B9CCA4F + (w54 = ((((w52 & 0xFFFFFFFF) >> 17) | (w52 << (32 - 17))) ^ (((w52 & 0xFFFFFFFF) >> 19) | (w52 << (32 - 19))) ^ ((w52 & 0xFFFFFFFF) >> 10)) + w47 + ((((w39 & 0xFFFFFFFF) >> 7) | (w39 << (32 - 7))) ^ (((w39 & 0xFFFFFFFF) >> 18) | (w39 << (32 - 18))) ^ ((w39 & 0xFFFFFFFF) >> 3)) + w38); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0x682E6FF3 + (w55 = ((((w53 & 0xFFFFFFFF) >> 17) | (w53 << (32 - 17))) ^ (((w53 & 0xFFFFFFFF) >> 19) | (w53 << (32 - 19))) ^ ((w53 & 0xFFFFFFFF) >> 10)) + w48 + ((((w40 & 0xFFFFFFFF) >> 7) | (w40 << (32 - 7))) ^ (((w40 & 0xFFFFFFFF) >> 18) | (w40 << (32 - 18))) ^ ((w40 & 0xFFFFFFFF) >> 3)) + w39); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };
            { temp1 = H + ((((E & 0xFFFFFFFF) >> 6) | (E << (32 - 6))) ^ (((E & 0xFFFFFFFF) >> 11) | (E << (32 - 11))) ^ (((E & 0xFFFFFFFF) >> 25) | (E << (32 - 25)))) + (G ^ (E & (F ^ G))) + 0x748F82EE + (w56 = ((((w54 & 0xFFFFFFFF) >> 17) | (w54 << (32 - 17))) ^ (((w54 & 0xFFFFFFFF) >> 19) | (w54 << (32 - 19))) ^ ((w54 & 0xFFFFFFFF) >> 10)) + w49 + ((((w41 & 0xFFFFFFFF) >> 7) | (w41 << (32 - 7))) ^ (((w41 & 0xFFFFFFFF) >> 18) | (w41 << (32 - 18))) ^ ((w41 & 0xFFFFFFFF) >> 3)) + w40); temp2 = ((((A & 0xFFFFFFFF) >> 2) | (A << (32 - 2))) ^ (((A & 0xFFFFFFFF) >> 13) | (A << (32 - 13))) ^ (((A & 0xFFFFFFFF) >> 22) | (A << (32 - 22)))) + ((A & B) | (C & (A | B))); D += temp1; H = temp1 + temp2; };
            { temp1 = G + ((((D & 0xFFFFFFFF) >> 6) | (D << (32 - 6))) ^ (((D & 0xFFFFFFFF) >> 11) | (D << (32 - 11))) ^ (((D & 0xFFFFFFFF) >> 25) | (D << (32 - 25)))) + (F ^ (D & (E ^ F))) + 0x78A5636F + (w57 = ((((w55 & 0xFFFFFFFF) >> 17) | (w55 << (32 - 17))) ^ (((w55 & 0xFFFFFFFF) >> 19) | (w55 << (32 - 19))) ^ ((w55 & 0xFFFFFFFF) >> 10)) + w50 + ((((w42 & 0xFFFFFFFF) >> 7) | (w42 << (32 - 7))) ^ (((w42 & 0xFFFFFFFF) >> 18) | (w42 << (32 - 18))) ^ ((w42 & 0xFFFFFFFF) >> 3)) + w41); temp2 = ((((H & 0xFFFFFFFF) >> 2) | (H << (32 - 2))) ^ (((H & 0xFFFFFFFF) >> 13) | (H << (32 - 13))) ^ (((H & 0xFFFFFFFF) >> 22) | (H << (32 - 22)))) + ((H & A) | (B & (H | A))); C += temp1; G = temp1 + temp2; };
            { temp1 = F + ((((C & 0xFFFFFFFF) >> 6) | (C << (32 - 6))) ^ (((C & 0xFFFFFFFF) >> 11) | (C << (32 - 11))) ^ (((C & 0xFFFFFFFF) >> 25) | (C << (32 - 25)))) + (E ^ (C & (D ^ E))) + 0x84C87814 + (w58 = ((((w56 & 0xFFFFFFFF) >> 17) | (w56 << (32 - 17))) ^ (((w56 & 0xFFFFFFFF) >> 19) | (w56 << (32 - 19))) ^ ((w56 & 0xFFFFFFFF) >> 10)) + w51 + ((((w43 & 0xFFFFFFFF) >> 7) | (w43 << (32 - 7))) ^ (((w43 & 0xFFFFFFFF) >> 18) | (w43 << (32 - 18))) ^ ((w43 & 0xFFFFFFFF) >> 3)) + w42); temp2 = ((((G & 0xFFFFFFFF) >> 2) | (G << (32 - 2))) ^ (((G & 0xFFFFFFFF) >> 13) | (G << (32 - 13))) ^ (((G & 0xFFFFFFFF) >> 22) | (G << (32 - 22)))) + ((G & H) | (A & (G | H))); B += temp1; F = temp1 + temp2; };
            { temp1 = E + ((((B & 0xFFFFFFFF) >> 6) | (B << (32 - 6))) ^ (((B & 0xFFFFFFFF) >> 11) | (B << (32 - 11))) ^ (((B & 0xFFFFFFFF) >> 25) | (B << (32 - 25)))) + (D ^ (B & (C ^ D))) + 0x8CC70208 + (w59 = ((((w57 & 0xFFFFFFFF) >> 17) | (w57 << (32 - 17))) ^ (((w57 & 0xFFFFFFFF) >> 19) | (w57 << (32 - 19))) ^ ((w57 & 0xFFFFFFFF) >> 10)) + w52 + ((((w44 & 0xFFFFFFFF) >> 7) | (w44 << (32 - 7))) ^ (((w44 & 0xFFFFFFFF) >> 18) | (w44 << (32 - 18))) ^ ((w44 & 0xFFFFFFFF) >> 3)) + w43); temp2 = ((((F & 0xFFFFFFFF) >> 2) | (F << (32 - 2))) ^ (((F & 0xFFFFFFFF) >> 13) | (F << (32 - 13))) ^ (((F & 0xFFFFFFFF) >> 22) | (F << (32 - 22)))) + ((F & G) | (H & (F | G))); A += temp1; E = temp1 + temp2; };
            { temp1 = D + ((((A & 0xFFFFFFFF) >> 6) | (A << (32 - 6))) ^ (((A & 0xFFFFFFFF) >> 11) | (A << (32 - 11))) ^ (((A & 0xFFFFFFFF) >> 25) | (A << (32 - 25)))) + (C ^ (A & (B ^ C))) + 0x90BEFFFA + (w60 = ((((w58 & 0xFFFFFFFF) >> 17) | (w58 << (32 - 17))) ^ (((w58 & 0xFFFFFFFF) >> 19) | (w58 << (32 - 19))) ^ ((w58 & 0xFFFFFFFF) >> 10)) + w53 + ((((w45 & 0xFFFFFFFF) >> 7) | (w45 << (32 - 7))) ^ (((w45 & 0xFFFFFFFF) >> 18) | (w45 << (32 - 18))) ^ ((w45 & 0xFFFFFFFF) >> 3)) + w44); temp2 = ((((E & 0xFFFFFFFF) >> 2) | (E << (32 - 2))) ^ (((E & 0xFFFFFFFF) >> 13) | (E << (32 - 13))) ^ (((E & 0xFFFFFFFF) >> 22) | (E << (32 - 22)))) + ((E & F) | (G & (E | F))); H += temp1; D = temp1 + temp2; };
            { temp1 = C + ((((H & 0xFFFFFFFF) >> 6) | (H << (32 - 6))) ^ (((H & 0xFFFFFFFF) >> 11) | (H << (32 - 11))) ^ (((H & 0xFFFFFFFF) >> 25) | (H << (32 - 25)))) + (B ^ (H & (A ^ B))) + 0xA4506CEB + (w61 = ((((w59 & 0xFFFFFFFF) >> 17) | (w59 << (32 - 17))) ^ (((w59 & 0xFFFFFFFF) >> 19) | (w59 << (32 - 19))) ^ ((w59 & 0xFFFFFFFF) >> 10)) + w54 + ((((w46 & 0xFFFFFFFF) >> 7) | (w46 << (32 - 7))) ^ (((w46 & 0xFFFFFFFF) >> 18) | (w46 << (32 - 18))) ^ ((w46 & 0xFFFFFFFF) >> 3)) + w45); temp2 = ((((D & 0xFFFFFFFF) >> 2) | (D << (32 - 2))) ^ (((D & 0xFFFFFFFF) >> 13) | (D << (32 - 13))) ^ (((D & 0xFFFFFFFF) >> 22) | (D << (32 - 22)))) + ((D & E) | (F & (D | E))); G += temp1; C = temp1 + temp2; };
            { temp1 = B + ((((G & 0xFFFFFFFF) >> 6) | (G << (32 - 6))) ^ (((G & 0xFFFFFFFF) >> 11) | (G << (32 - 11))) ^ (((G & 0xFFFFFFFF) >> 25) | (G << (32 - 25)))) + (A ^ (G & (H ^ A))) + 0xBEF9A3F7 + (w62 = ((((w60 & 0xFFFFFFFF) >> 17) | (w60 << (32 - 17))) ^ (((w60 & 0xFFFFFFFF) >> 19) | (w60 << (32 - 19))) ^ ((w60 & 0xFFFFFFFF) >> 10)) + w55 + ((((w47 & 0xFFFFFFFF) >> 7) | (w47 << (32 - 7))) ^ (((w47 & 0xFFFFFFFF) >> 18) | (w47 << (32 - 18))) ^ ((w47 & 0xFFFFFFFF) >> 3)) + w46); temp2 = ((((C & 0xFFFFFFFF) >> 2) | (C << (32 - 2))) ^ (((C & 0xFFFFFFFF) >> 13) | (C << (32 - 13))) ^ (((C & 0xFFFFFFFF) >> 22) | (C << (32 - 22)))) + ((C & D) | (E & (C | D))); F += temp1; B = temp1 + temp2; };
            { temp1 = A + ((((F & 0xFFFFFFFF) >> 6) | (F << (32 - 6))) ^ (((F & 0xFFFFFFFF) >> 11) | (F << (32 - 11))) ^ (((F & 0xFFFFFFFF) >> 25) | (F << (32 - 25)))) + (H ^ (F & (G ^ H))) + 0xC67178F2 + (w63 = ((((w61 & 0xFFFFFFFF) >> 17) | (w61 << (32 - 17))) ^ (((w61 & 0xFFFFFFFF) >> 19) | (w61 << (32 - 19))) ^ ((w61 & 0xFFFFFFFF) >> 10)) + w56 + ((((w48 & 0xFFFFFFFF) >> 7) | (w48 << (32 - 7))) ^ (((w48 & 0xFFFFFFFF) >> 18) | (w48 << (32 - 18))) ^ ((w48 & 0xFFFFFFFF) >> 3)) + w47); temp2 = ((((B & 0xFFFFFFFF) >> 2) | (B << (32 - 2))) ^ (((B & 0xFFFFFFFF) >> 13) | (B << (32 - 13))) ^ (((B & 0xFFFFFFFF) >> 22) | (B << (32 - 22)))) + ((B & C) | (D & (B | C))); E += temp1; A = temp1 + temp2; };

            #endregion

            state[0] += A;
            state[1] += B;
            state[2] += C;
            state[3] += D;
            state[4] += E;
            state[5] += F;
            state[6] += G;
            state[7] += H;
        }

    }
}
