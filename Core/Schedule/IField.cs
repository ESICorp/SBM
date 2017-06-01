namespace SBM.Schedule
{
    internal interface IField
    {
        int First { get; }
        int GetNext(int start);
        bool Contains(int value);
    }
}