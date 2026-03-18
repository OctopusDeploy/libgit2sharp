using LibGit2Sharp.Core;
using System;
using System.Runtime.InteropServices;

namespace LibGit2Sharp.Ssh
{
    internal static class NativeMethods
    {
        private const string libgit2 = NativeDllName.Name;

        [DllImport(libgit2, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int git_cred_ssh_key_new(
             out IntPtr cred,
             [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string username,
             [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string publickey,
             [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string privatekey,
             [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string passphrase);

        [DllImport(libgit2)]
        internal static extern int git_cred_ssh_key_memory_new(
              out IntPtr cred,
              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string username,
              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string publickey,
              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string privatekey,
              [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(StrictUtf8Marshaler))] string passphrase);
    }
}
