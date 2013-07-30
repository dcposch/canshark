using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSCP.Telem.CanShark
{
    public class TimeSeries
    {
        public string name { get; private set; }
        public List<DataPoint> points = new List<DataPoint>();
        public DataPoint LatestPoint
        {
            get
            {
                if(points.Count > 0)
                    return points[points.Count - 1];
                return new DataPoint();
            }
        }
        public TimeSeries(string name)
        {
            this.name = name;
        }
        public TimeSeries()
        {
        }

        public double ValueAt(DateTime time)
        {
            int ix = BSearch(time);
            return points[ix].Value;
        }
        public int BSearch(DateTime time)
        {
            return BSearch(points, 0, points.Count, time);
        }
        public int BSearch(List<DataPoint> points, int minIx, int maxIx, DateTime time)
        {
            if (minIx >= maxIx)
                return -1;
            if (minIx + 1 == maxIx)
                return minIx;
            if (minIx + 2 == maxIx)
                return points[minIx + 1].Timestamp > time ? minIx : minIx + 1;
            int midIx = (minIx + maxIx) / 2;
            if (points[midIx].Timestamp > time)
                return BSearch(points, minIx, midIx, time);
            return BSearch(points, midIx, maxIx, time);
        }

        public bool AddValue(DateTime timestamp, double value)
        {
            // Todo: fix this
            if (Math.Abs(value) > 20000)
                return false;

            if ((points.Count == 0) || (LatestPoint.Value != value))
            {
                DataPoint point = new DataPoint(timestamp, value);

                if (timestamp > LatestPoint.Timestamp)
                    points.Add(point);
                else
                {
                    int i;
                    for (i = points.Count - 1; i >= 0; i--)
                        if (points[i].Timestamp <= timestamp)
                            break;
                    points.Insert(i + 1, point);
                }

                return true;
            }

            return false;
        }
    }
    public struct DataPoint
    {
        public DateTime Timestamp { get; private set; }
        public double Value { get; private set; }

        public DataPoint(DateTime timestamp, double value) : this()
        {
            Timestamp = timestamp;
            Value = value;
        }
    }

    /// <summary>
    /// Represents a time series tracking multiple variables.
    /// </summary>
    public class MultiTimeSeries
    {
        Dictionary<string, TimeSeries> _series = new Dictionary<string,TimeSeries>();
        Dictionary<string, System.Delegate> _watchedVariables = new Dictionary<string, System.Delegate>();
        public delegate void TimeSeriesHandler(MultiTimeSeries sender, string var);
        public event TimeSeriesHandler DataPointReceived;
        public event TimeSeriesHandler VarAdded;
        public event TimeSeriesHandler VarRemoved;

        bool _playbackMode = false;

        public void AddDatapointChangedHandler(string name, TimeSeriesHandler handler)
        {
            if (!_watchedVariables.ContainsKey(name)) _watchedVariables.Add(name, null);
            _watchedVariables[name] = (TimeSeriesHandler)_watchedVariables[name] + handler;
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public void BeginPlayback()
        {
            _playbackMode = true;
        }
        public void EndPlayback()
        {
            _playbackMode = false;
        }

        /// <summary>
        /// Computes average value of a named variable in the closed interval [start, end]
        /// </summary>
        public double ComputeAverage(string name, DateTime start, DateTime end)
        {
            int n;
            return ComputeAverage(name, start, end, out n);
        }
        public double ComputeAverage(string name, DateTime start, DateTime end, out int nPoints)
        {
            nPoints = 0;
            if (!_series.ContainsKey(name))
                return 0.0;
            DateTime startMinusEpsilon = start.Subtract(new TimeSpan(1L));
            int ix1 = _series[name].BSearch(startMinusEpsilon) + 1;
            int ix2 = _series[name].BSearch(end);
            if (ix2 < ix1)
                return 0.0;
            double sum = 0.0;
            for (int i = ix1; i <= ix2; i++)
                sum += _series[name].points[i].Value;
            nPoints = ix1 - ix2 + 1;
            return sum / (double)nPoints;
        }

        /// <summary>
        /// Computes average value of a named variable in the closed interval [start, end]
        /// Each data point is weighted by the time since the point before it.
        /// </summary>
        public double ComputeTimeWeightedAverage(string name, DateTime start, DateTime end)
        {
            int n;
            return ComputeTimeWeightedAverage(name, start, end, out n);
        }
        public double ComputeTimeWeightedAverage(string name, DateTime start, DateTime end, out int nPoints)
        {
            nPoints = 0;
            if (!_series.ContainsKey(name))
                return 0.0;
            List<DataPoint> points = _series[name].points;
            DateTime startMinusEpsilon = start.Subtract(new TimeSpan(1L));
            int ix1 = _series[name].BSearch(startMinusEpsilon) + 1;
            int ix2 = _series[name].BSearch(end);
            if (ix2 <= ix1)
                return LatestValue(name, start);
            double sum = 0.0;
            DateTime prevTime = start;
            for (int i = ix1; i <= ix2; i++)
            {
                double prevValue = 0.0;
                if(i - 1 >= 0)
                    prevValue = points[i-1].Value;
                sum += prevValue * (double)(points[i].Timestamp.Ticks - prevTime.Ticks);
                prevTime = points[i].Timestamp;
            }
            sum += points[ix2].Value * (double)(end.Ticks - points[ix2].Timestamp.Ticks);
            nPoints = ix1 - ix2 + 1;
            return sum / (double)(end.Ticks - start.Ticks);
        }

        public double LatestValue(string name, DateTime time)
        {
            if (!_series.ContainsKey(name))
                return 0.0;
            int ix = _series[name].BSearch(time);
            if (ix < 0)
                return 0.0;
            return _series[name].points[ix].Value;
        }
        public double LatestValue(string name)
        {
            if (!_series.ContainsKey(name) || _series[name].points.Count == 0)
                return 0.0;
            return _series[name].points[_series[name].points.Count - 1].Value;
        }
        public TimeSeries this[string name]
        {
            get
            {
                if (!_series.ContainsKey(name))
                    return null;
                return _series[name];
            }
        }

        void updateMinMaxTime(DateTime timestamp)
        {
            if (StartTime.Ticks == 0)
                StartTime = timestamp;
            else if (timestamp < StartTime)
                StartTime = timestamp;
            if (timestamp > EndTime)
                EndTime = timestamp;
        }

        /// <summary>
        /// Adds a value to the end of a time series if the value has changed;
        /// </summary>
        public void AddIfDiff(string name, DateTime timestamp, double value)
        {
            if (!_series.ContainsKey(name)
                || _series[name].points.Count == 0
                || LatestValue(name) != value)
            {
                AddValue(name, timestamp, value);
            }
        }
        public void AddValue(string name, DateTime timestamp, double value)
        {
            bool add = !_series.ContainsKey(name);
            if (add)
            {
                _series.Add(name, new TimeSeries(name));
            }
            if (_series[name].AddValue(timestamp, value))
            {
                updateMinMaxTime(timestamp);

                if (!_playbackMode)
                {
                    if (add && VarAdded != null)
                        VarAdded(this, name);
                    if (DataPointReceived != null)
                        DataPointReceived(this, name);
                    if (_watchedVariables.ContainsKey(name))
                        ((TimeSeriesHandler)_watchedVariables[name])(this, name);
                }
            }
        }

        public void Clear()
        {
            while (_series.Count > 0)
            {
                string var = _series.Keys.First();
                if (VarRemoved != null)
                    VarRemoved(this, var);
                _series.Remove(var);
            }
            StartTime = EndTime = new DateTime();
        }

        public ICollection<string> Vars
        {
            get
            {
                return _series.Keys;
            }
        }
    }
}
