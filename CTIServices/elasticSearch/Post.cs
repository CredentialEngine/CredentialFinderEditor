using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace CTIServices.elasticSearch
{
	public class Post
	{
		public string Id { get; set; }

		public DateTime? CreationDate { get; set; }

		public int? Score { get; set; }

		public int? AnswerCount { get; set; }

		public string Body { get; set; }

		public string Title { get; set; }

		[String(Index = FieldIndexOption.NotAnalyzed)]
		public IEnumerable<string> Tags { get; set; }

		[Completion]
		public IEnumerable<string> Suggest { get; set; }
	}
}
