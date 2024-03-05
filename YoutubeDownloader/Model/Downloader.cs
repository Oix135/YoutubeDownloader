using System.IO;
using System.Net.Http;
using YoutubeExplode;

namespace YoutubeDownloader.Model
{
    internal class Downloader
    {
        internal async Task<string> Download(string url, string folder, int bitrate = 0 )
        {

            var client = new YoutubeClient();

            var video = await client.Videos.GetAsync(url);

            var filename = Path.Combine(folder, $"{video.Id}.mp4");

            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();


            if (muxedStreams.Count != 0)
            {
                var streamInfo = bitrate > 0 ? muxedStreams.Where(a=>a.VideoQuality.MaxHeight == bitrate).First() : muxedStreams.First();
                using var httpClient = new HttpClient();
                using var stream = await httpClient.GetStreamAsync(streamInfo.Url);
                using var outputStream = File.Create(filename);
                await stream.CopyToAsync(outputStream);
            }

            return filename;
        }

        internal async Task<Tuple<List<int>,string>> GetDescription(string url)
        {
            var client = new YoutubeClient();

            var video = await client.Videos.GetAsync(url);

            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).Select(a=>a.VideoQuality.MaxHeight).ToList();

            var description = $"Название: {video.Title}\nАвтор: {video.Author}\nОписание:{video.Description}";

            return new Tuple<List<int>, string>(muxedStreams, description);
        }
    }
}
