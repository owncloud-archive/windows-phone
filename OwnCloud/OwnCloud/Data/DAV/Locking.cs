using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwnCloud.Data.DAV
{
    class DAVLocking
    {
        /// <summary>
        /// Specifies whether a lock is an exclusive lock, or a shared lock.
        /// </summary>
        public enum LockScope
        {
            Exclusive = 1,
            Shared = 2
        }

        /// <summary>
        /// Specifies the access type of a lock. There is only one type supported: the write lock.
        /// </summary>
        public enum  LockType
        {
            /// <summary>
            /// Lock of type write.
            /// </summary>
            Write = 1
        }

        /// <summary>
        /// Gets or sets the locking scope.
        /// </summary>
        public LockScope Scope
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the locking type.
        /// </summary>
        public LockType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new locking object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="scope"></param>
        public DAVLocking(LockType type, LockScope scope)
        {
            Type = type;
            Scope = scope;
        }

        /// <summary>
        /// Creates a write-exlusive locking.
        /// </summary>
        public DAVLocking()
        {
            Type = LockType.Write;
            Scope = LockScope.Exclusive;
        }
    }
}
