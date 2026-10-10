namespace DatVeXemPhim.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resource, string field, object value)
        : base($"{resource} không tồn tại với {field} = {value}")
    {
    }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}

public class GheDaDatException : Exception
{
    public GheDaDatException(string message) : base(message)
    {
    }
}
