using System;
using LibGit2Sharp.Ssh;

namespace LibGit2Sharp
{
    /// <summary>
    /// Class that holds SSH username with in-memory key credentials for remote repository access.
    /// </summary>
    public sealed class SshUserKeyMemoryCredentials : Credentials
    {
        /// <summary>
        /// Callback to acquire a credential object.
        /// </summary>
        /// <param name="cred">The newly created credential object.</param>
        /// <returns>0 for success, &lt; 0 to indicate an error, &gt; 0 to indicate no credential was acquired.</returns>
        protected internal override int GitCredentialHandler(out IntPtr cred)
        {
            if (Username == null)
            {
                throw new InvalidOperationException("SshUserKeyMemoryCredentials contains a null Username.");
            }

            if (Passphrase == null)
            {
                throw new InvalidOperationException("SshUserKeyMemoryCredentials contains a null Passphrase.");
            }

            if (PublicKey == null)
            {
                // TODO: Can this be null or do we need to derive from the private key?
            }

            if (PrivateKey == null)
            {
                throw new InvalidOperationException("SshUserKeyMemoryCredentials contains a null PrivateKey.");
            }

            return NativeMethods.git_cred_ssh_key_memory_new(out cred, Username, PublicKey, PrivateKey, Passphrase);
        }

        /// <summary>
        /// Username for SSH authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Public key for SSH authentication.
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Private key for SSH authentication.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Passphrase for SSH authentication.
        /// </summary>
        public string Passphrase { get; set; }
    }
}
