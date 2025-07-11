using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Gifytools.Diagnostics
{

    public class GifyMetrics
    {
        public static ActivitySource GifytoolsActivitySource = new("Gifytools", typeof(GifyMetrics).Assembly.GetName().Version?.ToString());

        private readonly Counter<int> _gifsCreated;
        private readonly Histogram<double> _sizeHistogram;
        private readonly UpDownCounter<int> _currentJobs;

        public GifyMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("Gifytools");
            _gifsCreated = meter.CreateCounter<int>("gifytools.gifs.created", description: "Counts how many gifs were created");
            _sizeHistogram = meter.CreateHistogram<double>("gifytools.gifs.size", unit: "byte", description: "Records how big the converted videos are");
            _currentJobs = meter.CreateUpDownCounter<int>("gifytools.jobs.running", description: "The current number of conversion jobs running");
        }

        public void GifCreated()
        {
            _gifsCreated.Add(1);
        }

        public void VideoUploaded(double sizeOfVideo)
        {
            _sizeHistogram.Record(sizeOfVideo);
        }

        public void JobStarted()
        {
            _currentJobs.Add(1);
        }

        public void JobFinished()
        {
            _currentJobs.Add(-1);
        }
    }
}
