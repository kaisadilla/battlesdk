using battlesdk.data;
using battlesdk.world;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;
public static class G {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static World? World { get; private set; } = null;

    public static void LoadGame () {
        _logger.Info("Loaded game.");
        World = new(Registry.Maps[0], new(6, 4));
    }
}
