namespace egrants_new
{
   public interface IeEgantsUserSession
    {
      string Username { get; }

      void StartSession(string username);

      void End();
   }
}
