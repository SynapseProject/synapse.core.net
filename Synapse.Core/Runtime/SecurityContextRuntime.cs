using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

using Synapse.Core.Utilities;


namespace Synapse.Core
{
    //adapted from https://msdn.microsoft.com/en-us/library/w070t6ka(v=vs.110).aspx
    public partial class SecurityContext : ICrypto
    {
        [DllImport( "advapi32.dll", SetLastError = true )]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain,
            String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
        public extern static bool CloseHandle(IntPtr handle);

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        private IntPtr _token;
        private WindowsImpersonationContext _impContext = null;

        [PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
        public void Impersonate(CryptoProvider crypto)
        {
            if( !IsValid )
                return;

            if( !IsImpersonating )
            {
                _token = IntPtr.Zero;

                SecurityContext sc = this;
                if( HasCrypto && Crypto.HasElements )
                    sc = GetCryptoValues( crypto, isEncryptMode: false );

                bool ok = LogonUser( sc.UserName, sc.Domain, sc.Password,
                   LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref _token );

                if( HasCrypto )
                    sc = null;

                //try { password.Dispose(); } catch { }

                if( !ok )
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception( error );
                }

                WindowsIdentity identity = new WindowsIdentity( _token );
                _impContext = identity.Impersonate();
            }
        }

        [PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
        public void Undo()
        {
            if( IsImpersonating )
            {
                _impContext.Undo();

                if( _token != IntPtr.Zero )
                    CloseHandle( _token );

                _impContext = null;
            }
        }

        public bool IsImpersonating { get { return _impContext != null; } }

        public SecurityContext GetCryptoValues(CryptoProvider planCrypto = null, bool isEncryptMode = true)
        {
            if( HasCrypto )
                Crypto.InheritSettingsIfRequired( planCrypto );
            return YamlHelpers.GetCryptoValues( this, isEncryptMode );
        }
    }
}