using System.Collections.Generic;

namespace SearchCore.Services
{
	public class ServiceResult
	{
		public bool Success => Errors.Count == 0;
		public List<string> Errors { get; set; } = [];
	}

	public class ServiceResult<T> : ServiceResult
	{
		public T? Data { get; set; }
	}
}
