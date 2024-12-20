using System;

public class IDGenerator
{
    public static int GenerateID()
    {
        Guid guid = Guid.NewGuid();
        int hashCode = guid.GetHashCode();
        return Math.Abs(hashCode); // Ensure the ID is positive
    }
}