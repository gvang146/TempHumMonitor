using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using TempAndHumidityReaderService.Settings;
using TempAndHumidityReaderService.Structure;

namespace TempAndHumidityReaderService
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        static async Task Main(string[] args)
        {
            
            if (args.Length == 0 ||
                string.IsNullOrEmpty(args[0]))
            {
                const string invalidArgMsg = "No argument specified: please specify either '-console' or '-service' as running mode";
                _logger.Error(invalidArgMsg);
                Console.WriteLine(invalidArgMsg);
                return;
            }

            var options = new[] { "-console", "-service" };
            var option = args[0];
            if (!options.Contains(option))
            {
                const string invalidArgMsg = "Invalid argument specified: please specify either '-console' or '-service' as running mode";
                _logger.Error(invalidArgMsg);
                Console.WriteLine(invalidArgMsg);
                return;
            }
            
            var program = new Program();
            await program.Run(option);
        }

        private async Task Run(string option)
        {
            const string startingMsg = "Starting application...";
            _logger.Debug(startingMsg);
            Console.WriteLine(startingMsg);
            
            try
            {
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var settingDir = Path.GetDirectoryName(exePath);
                var jsonStr = await File.ReadAllTextAsync($"{settingDir}/appsettings.json");
                var appSetting = JsonSerializer.Deserialize<AppSetting>(jsonStr);

                if (appSetting != null)
                {
                    var device = new Device(appSetting.Device);
                    device.ConnectToWebApi();
                    device.StartMonitoring();

                    const string startedMsg = "Application has started...";
                    _logger.Debug(startedMsg);
                    Console.WriteLine(startedMsg);
                    
                    switch (option)
                    {
                        case "-console":
                            Console.Write("Press ENTER to terminate...");

                            try
                            {
                                ConsoleKeyInfo keyInfo;
                                do
                                {
                                    keyInfo = Console.ReadKey();
                                } while (keyInfo.Key != ConsoleKey.Enter);
                            }
                            catch (Exception e)
                            {
                                var innerErrMsg = $"Interrupted error has occurred. ERR: {e}";
                                _logger.Error(innerErrMsg);
                                Console.WriteLine(innerErrMsg);
                            }
                            
                            Console.WriteLine();
                            break;
                        case "-service":
                            try
                            {
                                while (true) await Task.Delay(500);
                            }
                            catch (Exception e)
                            {
                                var innerErrMsg = $"Interrupted error has occurred. ERR: {e}";
                                _logger.Error(innerErrMsg);
                                Console.WriteLine(innerErrMsg);
                            }
                            break;
                    }
                    
                    device.StopMonitoring();
                }
            }
            catch (Exception e)
            {
                var errMsg = $"Error starting application. ERR: {e}";
                _logger.Error(errMsg);
                Console.WriteLine(errMsg);
            }

            const string terminatedMsg = "Application terminated...";
            _logger.Debug(terminatedMsg);
            Console.WriteLine(terminatedMsg);
        }
    }
}