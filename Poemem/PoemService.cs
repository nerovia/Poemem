using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Poemem.Common;

namespace Poemem
{
    internal static class PoemService
	{
		public static async Task<Poem> FetchFromPoetryDB(string query)
		{
			const string BaseUrl = "https://poetrydb.org";
			var title = query.Trim().ToLower();

			using (var client = new HttpClient())
			{
				var url = $"{BaseUrl}/title/{title}";
				Debug.WriteLine($"request poem '{title}' from '{url}'");
				var poems = await client.GetFromJsonAsync<Poem[]>(url);
				return poems!.Single();
			}
		}

		public static async Task<Poem> FetchFromLocal(string path)
		{
			if (!File.Exists(path))
				throw new HandleException("Could not find file.");

			switch (Path.GetExtension(path))
			{
				case ".json":
					using (var stream = File.OpenRead(path))
						return await JsonSerializer.DeserializeAsync<Poem>(stream) ?? throw new Exception();
				case ".txt":
					var lines = await File.ReadAllLinesAsync(path);
					var size = Array.FindLastIndex(lines, it => !string.IsNullOrEmpty(it)) + 1;
					if (size < lines.Length)
						Array.Resize(ref lines, size);

					return new Poem()
					{
						Title = Path.GetFileNameWithoutExtension(path).ToTitleCase(),
						Author = "unspecified",
						Lines = lines
					};
				default:
					throw new Exception();
			};			
		}
	}
}
