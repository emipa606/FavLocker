using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locker;

public class ContentsRenderer(Building_PowerArmorStation powerArmorStation)
{
    private readonly List<ApparelGraphicRecord> apparelGraphics = [];

    private readonly List<ApparelGraphicRecord> apparelGraphicsOverhead = [];

    private readonly CompLocker compLocker = powerArmorStation.GetComp<CompLocker>();
    private readonly Quaternion quatNoRotation = Quaternion.AngleAxis(0f, Vector3.up);

    private bool allResolved;

    public void RenderContents(Vector3 rootLoc)
    {
        if (!allResolved)
        {
            ResolveApparelGraphics();
        }

        drawOverhead(rootLoc);
        drawBody(rootLoc);
    }

    private void drawOverhead(Vector3 rootLoc)
    {
        var rotation = powerArmorStation.Rotation;
        var mesh = getHairMeshSet().MeshAt(rotation);
        var vector = quatNoRotation * baseHeadOffsetAt(rotation);
        var loc = adjustLoctaionToStandbyPosition(rootLoc) + vector;
        foreach (var item in apparelGraphicsOverhead)
        {
            loc.y += rotation == Rot4.North ? 0.003787879f : 3f / 88f;
            var mat2 = item.graphic.MatAt(rotation);
            GenDraw.DrawMeshNowOrLater(mesh, loc, quatNoRotation, mat2, false);
        }
    }

    private void drawBody(Vector3 rootLoc)
    {
        var rotation = powerArmorStation.Rotation;
        var mesh = MeshPool.humanlikeMeshSet_Custom.FirstOrDefault().Value.MeshAt(rotation);
        var loc = adjustLoctaionToStandbyPosition(rootLoc);
        foreach (var apparelGraphic in apparelGraphics)
        {
            var mat = apparelGraphic.graphic.MatAt(rotation);
            GenDraw.DrawMeshNowOrLater(mesh, loc, quatNoRotation, mat, false);
            loc.y += 0.003787879f;
        }
    }

    private Vector3 adjustLoctaionToStandbyPosition(Vector3 rootLoc)
    {
        var standbyPosition = powerArmorStation.GetStandbyPosition();
        var result = new Vector3(standbyPosition.x + 0.5f, rootLoc.y, standbyPosition.z + 1f);
        if (powerArmorStation.Rotation == Rot4.North)
        {
            result.y -= 1f;
        }

        return result;
    }

    public void ResolveApparelGraphics()
    {
        apparelGraphics.Clear();
        apparelGraphicsOverhead.Clear();
        foreach (var item in sortedApparelListForDraw())
        {
            if (!ApparelGraphicRecordGetter.TryGetGraphicApparel(item, getBodyTypeDef(), true, out var rec))
            {
                continue;
            }

            if (rec.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            {
                apparelGraphicsOverhead.Add(rec);
            }
            else
            {
                apparelGraphics.Add(rec);
            }
        }

        allResolved = true;
    }

    private List<Apparel> sortedApparelListForDraw()
    {
        return Util.SortApparelListForDraw(compLocker.InnerApparelsReadOnly());
    }

    public void SetApparelGraphicsDirty()
    {
        if (allResolved)
        {
            ResolveApparelGraphics();
        }
    }

    private Vector3 baseHeadOffsetAt(Rot4 rotation)
    {
        var headOffset = getBodyTypeDef().headOffset;
        switch (rotation.AsInt)
        {
            case 0:
                return new Vector3(0f, 0f, headOffset.y);
            case 1:
                return new Vector3(headOffset.x, 0f, headOffset.y);
            case 2:
                return new Vector3(0f, 0f, headOffset.y);
            case 3:
                return new Vector3(0f - headOffset.x, 0f, headOffset.y);
            default:
            {
                var owner = getOwner();
                Log.Error($"BaseHeadOffsetAt error in {(owner == null ? "defaultBodyTypeDef" : owner.ToString())}");
                return Vector3.zero;
            }
        }
    }

    private static GraphicMeshSet getHairMeshSet()
    {
        return MeshPool.GetMeshSetForSize(MeshPool.HumanlikeHeadAverageWidth, MeshPool.HumanlikeHeadAverageWidth);
    }

    private BodyTypeDef getBodyTypeDef()
    {
        var owner = getOwner();
        return owner == null ? BodyTypeDefOf.Male : owner.story.bodyType;
    }

    private Pawn getOwner()
    {
        return powerArmorStation.GetComp<CompAssignableToPawn_Locker>().AssignedPawn();
    }
}