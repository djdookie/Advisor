namespace HDT.Plugins.Advisor.Services
{
	public interface ILoggingService
	{
		void Error(string message);

		void Error(object obj);

		void Info(string message);

		void Info(object obj);

		void Debug(string message);

		void Debug(object obj);
	}
}