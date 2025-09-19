using System.Net;
using System.Runtime.InteropServices;
namespace TheAccelerator.Libraries.Network
{
	public class NetworkConnection : IDisposable
	{
		private readonly string _networkName;

		public NetworkConnection(string networkName, NetworkCredential credentials)
		{
			_networkName = networkName ?? throw new ArgumentNullException(nameof(networkName));

			var netResource = new NetResource
			{
				dwType = ResourceType.Disk,
				lpRemoteName = networkName
			};

			var result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);
			if (result != 0)
				throw new InvalidOperationException($"Error connecting to network share {networkName}, code {result}");
		}

		public void Dispose()
		{
			WNetCancelConnection2(_networkName, 0, true);
		}

		#region P/Invoke

		[DllImport("mpr.dll")]
		private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

		[DllImport("mpr.dll")]
		private static extern int WNetCancelConnection2(string name, int flags, bool force);

		[StructLayout(LayoutKind.Sequential)]
		private class NetResource
		{
			public ResourceScope dwScope = ResourceScope.GlobalNetwork;
			public ResourceType dwType;
			public ResourceDisplayType dwDisplayType = ResourceDisplayType.Share;
			public ResourceUsage dwUsage = 0;
			public string lpLocalName = null;
			public string lpRemoteName = null;
			public string lpComment = null;
			public string lpProvider = null;
		}

		private enum ResourceScope : int
		{
			Connected = 1,
			GlobalNetwork,
			Remembered,
			Recent,
			Context
		}

		private enum ResourceType : int
		{
			Any = 0,
			Disk = 1,
			Print = 2,
			Reserved = 8,
		}

		private enum ResourceDisplayType : int
		{
			Generic = 0x0,
			Domain = 0x01,
			Server = 0x02,
			Share = 0x03,
			File = 0x04,
			Group = 0x05,
			Network = 0x06,
			Root = 0x07,
			Shareadmin = 0x08,
			Directory = 0x09,
			Tree = 0x0a,
			Ndscontainer = 0x0b
		}

		[Flags]
		private enum ResourceUsage : int
		{
			Connectable = 0x00000001,
			Container = 0x00000002,
			NoLocalDevice = 0x00000004,
			Sibling = 0x00000008,
			Attached = 0x00000010,
			All = Connectable | Container | Attached
		}

		#endregion
	}
}
