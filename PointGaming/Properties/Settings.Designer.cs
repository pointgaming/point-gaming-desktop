﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PointGaming.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Username {
            get {
                return ((string)(this["Username"]));
            }
            set {
                this["Username"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://socket.pointgaming.com/")]
        public string SocketIoUrl {
            get {
                return ((string)(this["SocketIoUrl"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShouldFlashChatWindow {
            get {
                return ((bool)(this["ShouldFlashChatWindow"]));
            }
            set {
                this["ShouldFlashChatWindow"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:10")]
        public global::System.TimeSpan LogInTimeout {
            get {
                return ((global::System.TimeSpan)(this["LogInTimeout"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Segoe UI")]
        public string ChatFontFamily {
            get {
                return ((string)(this["ChatFontFamily"]));
            }
            set {
                this["ChatFontFamily"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public double ChatFontSize {
            get {
                return ((double)(this["ChatFontSize"]));
            }
            set {
                this["ChatFontSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection LaunchList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["LaunchList"]));
            }
            set {
                this["LaunchList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double HomeWindowBoundsWidth {
            get {
                return ((double)(this["HomeWindowBoundsWidth"]));
            }
            set {
                this["HomeWindowBoundsWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double HomeWindowBoundsHeight {
            get {
                return ((double)(this["HomeWindowBoundsHeight"]));
            }
            set {
                this["HomeWindowBoundsHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double HomeWindowBoundsLeft {
            get {
                return ((double)(this["HomeWindowBoundsLeft"]));
            }
            set {
                this["HomeWindowBoundsLeft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double HomeWindowBoundsTop {
            get {
                return ((double)(this["HomeWindowBoundsTop"]));
            }
            set {
                this["HomeWindowBoundsTop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string HomeWindowBoundsDesktopInfo {
            get {
                return ((string)(this["HomeWindowBoundsDesktopInfo"]));
            }
            set {
                this["HomeWindowBoundsDesktopInfo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ChatWindowBoundsWidth {
            get {
                return ((double)(this["ChatWindowBoundsWidth"]));
            }
            set {
                this["ChatWindowBoundsWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ChatWindowBoundsHeight {
            get {
                return ((double)(this["ChatWindowBoundsHeight"]));
            }
            set {
                this["ChatWindowBoundsHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ChatWindowBoundsLeft {
            get {
                return ((double)(this["ChatWindowBoundsLeft"]));
            }
            set {
                this["ChatWindowBoundsLeft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ChatWindowBoundsTop {
            get {
                return ((double)(this["ChatWindowBoundsTop"]));
            }
            set {
                this["ChatWindowBoundsTop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ChatWindowBoundsDesktopInfo {
            get {
                return ((string)(this["ChatWindowBoundsDesktopInfo"]));
            }
            set {
                this["ChatWindowBoundsDesktopInfo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Password {
            get {
                return ((string)(this["Password"]));
            }
            set {
                this["Password"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpdateAutomatic {
            get {
                return ((bool)(this["UpdateAutomatic"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50.116.28.30")]
        public string StratumIp {
            get {
                return ((string)(this["StratumIp"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3334")]
        public int StratumPort {
            get {
                return ((int)(this["StratumPort"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public double BitcoinMinerUserIdleMinutes {
            get {
                return ((double)(this["BitcoinMinerUserIdleMinutes"]));
            }
            set {
                this["BitcoinMinerUserIdleMinutes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool BitcoinMinerOnlyWheUserIdle {
            get {
                return ((bool)(this["BitcoinMinerOnlyWheUserIdle"]));
            }
            set {
                this["BitcoinMinerOnlyWheUserIdle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool BitcoinMinerEnabled {
            get {
                return ((bool)(this["BitcoinMinerEnabled"]));
            }
            set {
                this["BitcoinMinerEnabled"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://dev.pointgaming.com")]
        public string WebServerUrl {
            get {
                return ((string)(this["WebServerUrl"]));
            }
        }
    }
}
