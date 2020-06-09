using Se7en.OpenCl.Api.Enum;
using System;
using System.Runtime.InteropServices;

namespace Se7enCl.Api
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommandQueueProperty
    {
        public static CommandQueueProperty Zero { get; } = new CommandQueueProperty(0);

        private readonly uint _propertyName;
        private readonly IntPtr _propertyValue;

        public CommandQueueProperty(CommandQueueProperties property, IntPtr value)
        {
            _propertyName = (uint)property;
            _propertyValue = value;
        }

        public CommandQueueProperty(CommandQueueProperties property)
        {
            _propertyName = (uint)property;
            _propertyValue = IntPtr.Zero;
        }
    }
}