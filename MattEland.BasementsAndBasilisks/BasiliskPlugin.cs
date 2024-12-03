using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks;

public abstract class BasiliskPlugin
{
    protected BasiliskPlugin(RequestContextService context)
    {
        Context = context;
    }
    
    protected RequestContextService Context { get; }

    public Kernel? Kernel { get; set; }
}