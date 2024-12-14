using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster;

public abstract class GamePlugin
{
    protected GamePlugin(RequestContextService context)
    {
        Context = context;
    }
    
    protected RequestContextService Context { get; }

    public Kernel? Kernel { get; set; }
}