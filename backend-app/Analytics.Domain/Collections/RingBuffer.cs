using Analytics.Domain.Entities;

namespace Analytics.Domain.Collections;

public class RingBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacity;
    private int _head;
    private int _tail;
    private int _count;
    private readonly object _lock = new();

    public RingBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }

    public int Capacity => _capacity;

    public void Add(T item)
    {
        lock (_lock)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _capacity;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                _tail = (_tail + 1) % _capacity;
            }
        }
    }

    public T[] GetAll()
    {
        lock (_lock)
        {
            if (_count == 0)
                return Array.Empty<T>();

            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                int index = (_tail + i) % _capacity;
                result[i] = _buffer[index];
            }
            return result;
        }
    }

    public T[] GetLatest(int count)
    {
        lock (_lock)
        {
            if (_count == 0)
                return Array.Empty<T>();

            int takeCount = Math.Min(count, _count);
            var result = new T[takeCount];
            
            for (int i = 0; i < takeCount; i++)
            {
                int index = (_head - takeCount + i + _capacity) % _capacity;
                result[i] = _buffer[index];
            }
            return result;
        }
    }

    public T[] GetRange(DateTime from, DateTime to)
    {
        lock (_lock)
        {
            if (_count == 0)
                return Array.Empty<T>();

            var result = new List<T>();
            for (int i = 0; i < _count; i++)
            {
                int index = (_tail + i) % _capacity;
                var item = _buffer[index];
                
                if (item is SensorReading reading)
                {
                    if (reading.Timestamp >= from && reading.Timestamp <= to)
                    {
                        result.Add(item);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
