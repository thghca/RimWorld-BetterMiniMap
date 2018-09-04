using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace BetterMiniMap.Overlays
{
    public class ShipsOverlay : ThingOverlay, IExposable
    {
        public ShipsOverlay() : base(null) { }

        public override int GetUpdateInterval() => 60;

        public new IEnumerable<Thing> GetThings()
        {
            return Find.CurrentMap.listerThings.AllThings.Where(t => t is Building_CrashedShipPart);
        }

        public override void CreateMarker(Thing thing, float edgeOpacity = 0.5f)
        {
            base.CreateMarker(thing.Position, 2f, Color.red, Color.red, edgeOpacity);
        }

        public new void ExposeData() => this.ExposeData("overlayShips");

        public override void Render()
        {
            foreach (Thing current in this.GetThings())
                this.CreateMarker(current);
        }
    }
}