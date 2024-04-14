using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locker;

public class ContentsRenderer(Building_PowerArmorStation powerArmorStation)
{
    private readonly List<ApparelGraphicRecord> apparelGraphics = [];

    private readonly List<ApparelGraphicRecord> apparelGraphicsOverhead = [];

    private readonly CompLocker compLocker = powerArmorStation.GetComp<CompLocker>();
    private readonly Building_PowerArmorStation powerArmorStation = powerArmorStation;
    private readonly Quaternion QUAT_NO_ROTATION = Quaternion.AngleAxis(0f, Vector3.up);

    private bool allResolved;

    public void RenderContents(Vector3 rootLoc)
    {
        if (!allResolved)
        {
            ResolveApparelGraphics();
        }

        DrawOverhead(rootLoc);
        DrawBody(rootLoc);
    }

    private void DrawOverhead(Vector3 rootLoc)
    {
        Rot4 rotation = powerArmorStation.Rotation;
        Mesh mesh = MeshPool.plane14;
        Vector3 vector = QUAT_NO_ROTATION * BaseHeadOffsetAt(rotation);
        Vector3 loc = AdjustLoctaionToStandbyPosition(rootLoc) + vector;
        foreach (ApparelGraphicRecord item in apparelGraphicsOverhead)
        {
            //if (item.sourceApparel.def.apparel.hatRenderedFrontOfFace)
            //{
            //    loc.y += 0.030303031f;
            //    var mat = item.graphic.MatAt(rotation);
            //    GenDraw.DrawMeshNowOrLater(mesh, loc, QUAT_NO_ROTATION, mat, false);
            //}
            //else
            //{
            loc.y += rotation == Rot4.North ? 0.003787879f : 3f / 88f;
            Material mat = item.graphic.MatAt(rotation);
            GenDraw.DrawMeshNowOrLater(mesh, loc, QUAT_NO_ROTATION, mat, false);
            //}
        }
    }

    private void DrawBody(Vector3 rootLoc)
    {
        Rot4 rotation = powerArmorStation.Rotation;
        Mesh mesh = MeshPool.plane14;
        Vector3 loc = AdjustLoctaionToStandbyPosition(rootLoc);
        foreach (var apparelGraphic in apparelGraphics)
        {
            Material mat = apparelGraphic.graphic.MatAt(rotation);
            GenDraw.DrawMeshNowOrLater(mesh, loc, QUAT_NO_ROTATION, mat, false);
            loc.y += 0.003787879f;
        }
    }

    private Vector3 AdjustLoctaionToStandbyPosition(Vector3 rootLoc)
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
        foreach (var item in SortedApparelListForDraw())
        {
            if (!ApparelGraphicRecordGetter.TryGetGraphicApparel(item, GetBodyTypeDef(), out var rec))
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

    private List<Apparel> SortedApparelListForDraw()
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

    public Vector3 BaseHeadOffsetAt(Rot4 rotation)
    {
        var headOffset = GetBodyTypeDef().headOffset;
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
                    var owner = GetOwner();
                    Log.Error($"BaseHeadOffsetAt error in {(owner == null ? "defaultBodyTypeDef" : owner.ToString())}");
                    return Vector3.zero;
                }
        }
    }

    private BodyTypeDef GetBodyTypeDef()
    {
        var owner = GetOwner();
        if (owner == null)
        {
            return BodyTypeDefOf.Male;
        }

        return owner.story.bodyType;
    }

    private Pawn GetOwner()
    {
        return powerArmorStation.GetComp<CompAssignableToPawn_Locker>().AssignedPawn();
    }
}