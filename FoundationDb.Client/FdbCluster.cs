﻿#region BSD Licence
/* Copyright (c) 2013, Doxense SARL
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of the <organization> nor the
	  names of its contributors may be used to endorse or promote products
	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using FoundationDb.Client.Native;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace FoundationDb.Client
{

	/// <summary>FoundationDB Cluster</summary>
	/// <remarks>Wraps an FDBCluster* handle</remarks>
	public class FdbCluster : IDisposable
	{

		private ClusterHandle m_handle;
		private bool m_disposed;

		internal FdbCluster(ClusterHandle handle)
		{
			m_handle = handle;
		}

		internal ClusterHandle Handle { get { return m_handle; } }

		private void ThrowIfDisposed()
		{
			if (m_disposed) throw new ObjectDisposedException(null);
		}

		public void Dispose()
		{
			if (!m_disposed)
			{
				m_disposed = true;
				m_handle.Dispose();
			}
		}

		public Task<FdbDatabase> OpenDatabaseAsync(string databaseName, CancellationToken ct = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(databaseName)) throw new ArgumentNullException("databaseName");

			var future = FdbNativeStub.ClusterCreateDatabase(m_handle, databaseName);

			return FdbFuture.CreateTaskFromHandle(
				future,
				(h) =>
				{
					DatabaseHandle database;
					var err = FdbNativeStub.FutureGetDatabase(h, out database);
					if (err != FdbError.Success)
					{
						database.Dispose();
						throw FdbCore.MapToException(err);
					}
					Debug.WriteLine("FutureGetDatabase => 0x" + database.Handle.ToString("x"));

					return new FdbDatabase(this, database, databaseName);
				},
				ct
			);
		}

	}

}