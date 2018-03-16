using StudioForge.BlockWorld;
using StudioForge.TotalMiner;
using StudioForge.TotalMiner.API;

namespace RandomBricksArcade
{
    class BrickbreakerPlugin : ITMPluginArcade
    {
        #region ITMPluginArcade

        ArcadeMachine ITMPluginArcade.GetArcadeMachine(int gameID, ITMGame game, ITMMap map, ITMPlayer player, GlobalPoint3D p, BlockFace face)
        {
            return new BrickbreakerGame(game, map, player, p, face);
        }

        IArcadeMachineRenderer ITMPluginArcade.GetArcadeMachineRenderer(int gameID)
        {
            var renderer = new BrickbreakerRenderer();
            renderer.LoadContent(null);
            return renderer;
        }

        string ITMPluginArcade.GetArcadeMachineName(int gameID)
        {
            return "Random Bricks";
        }

        #endregion
    }
}
