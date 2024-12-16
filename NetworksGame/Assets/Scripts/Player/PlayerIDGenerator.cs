using System;

public class PlayerIDGenerator
{
    public static int GeneratePlayerID()
    {
        Guid guid = Guid.NewGuid();
        int hashCode = guid.GetHashCode();
        return Math.Abs(hashCode); // Ensure the ID is positive
    }
}