namespace OwnCloud.Data.Calendar.Parsing
{
    interface IParser<out T, TIn>
    {
        T Parse(TIn value);
    }

    interface IParser<out T> : IParser<T,string>
    {
         
    }

}
