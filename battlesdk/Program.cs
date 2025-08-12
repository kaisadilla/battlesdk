global using battlesdk.types;
global using static battlesdk.types.TypesExtension;

//using battlesdk;
//using NLog;
//
//Logger _logger = LogManager.GetCurrentClassLogger();
//_logger.Info("Launching BattleSDK.");
//
//Settings.Load("res/battlesdk.toml");
//
//Game game;
//
//try {
//    game = new();
//}
//catch (Exception ex) {
//    _logger.FatalEx(ex, $"A fatal error occurred during initialization.");
//
//    Environment.Exit(1);
//    return;
//}
//
//while (game.CloseRequested == false) {
//    try {
//        game.NextFrame();
//    }
//    catch (Exception ex) {
//        _logger.FatalEx(ex, $"An unrecoverable error ocurred in the program loop.");
//
//        game.Close();
//
//        Environment.Exit(1);
//        return;
//    }
//}
//
//game.Close();
//