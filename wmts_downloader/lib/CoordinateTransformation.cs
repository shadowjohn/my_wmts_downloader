using System;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;


namespace CoordinateTransformation
{
    public class CoordinateTransformation
    {
        public static ICoordinateTransformation TransformCoordinate(String aSource, String aTarget)
        {
            var ctf = new CoordinateTransformationFactory();
            var cf = new CoordinateSystemFactory();
            var epsg3825 = cf.CreateFromWkt("PROJCS[\"TWD97 / TM2 zone 119\",GEOGCS[\"TWD97\",DATUM[\"D_Taiwan Datum 1997\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse Mercator\"],PARAMETER[\"central_meridian\",119.0],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"scale_factor\",0.9999],PARAMETER[\"false_easting\",250000.0],PARAMETER[\"false_northing\",0.0],UNIT[\"m\",1.0]]");
            var epsg3826 = cf.CreateFromWkt("PROJCS[\"TWD97 / TM2 zone 121\",GEOGCS[\"TWD97\",DATUM[\"D_Taiwan Datum 1997\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse Mercator\"],PARAMETER[\"central_meridian\",121.0],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"scale_factor\",0.9999],PARAMETER[\"false_easting\",250000.0],PARAMETER[\"false_northing\",0.0],UNIT[\"m\",1.0]]");
            var epsg3827 = cf.CreateFromWkt("PROJCS[\"TWD67 / TM2 zone 119\",GEOGCS[\"TWD67\",DATUM[\"D_Taiwan Datum 1967\",SPHEROID[\"GRS_1967_Modified\",6378160.0,298.25]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse Mercator\"],PARAMETER[\"central_meridian\",119.0],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"scale_factor\",0.9999],PARAMETER[\"false_easting\",250000.0],PARAMETER[\"false_northing\",0.0],UNIT[\"m\",1.0]]");
            var epsg3828 = cf.CreateFromWkt("PROJCS[\"TWD67 / TM2 zone 121\",GEOGCS[\"TWD67\",DATUM[\"D_Taiwan Datum 1967\",SPHEROID[\"GRS_1967_Modified\",6378160.0,298.25]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse Mercator\"],PARAMETER[\"central_meridian\",121.0],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"scale_factor\",0.9999],PARAMETER[\"false_easting\",250000.0],PARAMETER[\"false_northing\",0.0],UNIT[\"m\",1.0]]");
            var epsg3857 = cf.CreateFromWkt("PROJCS[\"Google Mercator\",GEOGCS[\"WGS 84\",DATUM[\"World Geodetic System 1984\",SPHEROID[\"WGS 84\",6378137.0,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0.0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.017453292519943295],AXIS[\"Geodetic latitude\",NORTH],AXIS[\"Geodetic longitude\",EAST],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Mercator_1SP\"],PARAMETER[\"semi_minor\",6378137.0],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"central_meridian\",0.0],PARAMETER[\"scale_factor\",1.0],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",0.0],UNIT[\"m\",1.0],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],AUTHORITY[\"EPSG\",\"900913\"]]");
            var epsg3857mod = cf.CreateFromWkt("PROJCS[\"WGS 84 / Pseudo-Mercator\", GEOGCS[\"WGS 84\", DATUM[\"WGS_1984\", SPHEROID[\"WGS 84\", 6378137, 298.257223563, AUTHORITY[\"EPSG\", \"7030\"]], AUTHORITY[\"EPSG\", \"6326\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.01745329251994328, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4326\"]], PROJECTION[\"Mercator_1SP\"], PARAMETER[\"latitude_of_origin\", 0], PARAMETER[\"central_meridian\", 0], PARAMETER[\"scale_factor\", 1], PARAMETER[\"false_easting\", 0], PARAMETER[\"false_northing\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"X\", EAST], AXIS[\"Y\", NORTH], AUTHORITY[\"EPSG\", \"3857\"]]");
            var epsg4326 = cf.CreateFromWkt("GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            var epsg102443 = cf.CreateFromWkt("PROJCS[\"TWD_1997_TM_Taiwan\",GEOGCS[\"GCS_TWD_1997\",DATUM[\"D_TWD_1997\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",250000.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",121.0],PARAMETER[\"Scale_Factor\",0.9999],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]");
            var epsg900913 = cf.CreateFromWkt("PROJCS[\"Google_Mercator\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,0]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Mercator\"],PARAMETER[\"latitude_of_origin\",0.0],PARAMETER[\"False_Easting\",0],PARAMETER[\"False_Northing\",0],PARAMETER[\"Central_Meridian\",0],PARAMETER[\"Standard_Parallel_1\",0],UNIT[\"Meter\",1]]");
            ICoordinateSystem sCoorSys = null;
            ICoordinateSystem tCoorSys = null;
            switch (aSource)
            {
                case "3825":
                    sCoorSys = epsg3825;
                    break;
                case "3826":
                    sCoorSys = epsg3826;
                    break;
                case "3827":
                    sCoorSys = epsg3827;
                    break;
                case "3828":
                    sCoorSys = epsg3828;
                    break;
                case "3857":
                    sCoorSys = epsg3857;
                    break;
                case "4326":
                    sCoorSys = epsg4326;
                    break;
                case "102443":
                    sCoorSys = epsg102443;
                    break;
                case "900913":
                    sCoorSys = epsg900913;
                    break;
                default:
                    break;
            }
            switch (aTarget)
            {
                case "3825":
                    tCoorSys = epsg3825;
                    break;
                case "3826":
                    tCoorSys = epsg3826;
                    break;
                case "3827":
                    tCoorSys = epsg3827;
                    break;
                case "3828":
                    tCoorSys = epsg3828;
                    break;
                case "3857":
                    tCoorSys = epsg3857;
                    break;
                case "4326":
                    tCoorSys = epsg4326;
                    break;
                case "102443":
                    tCoorSys = epsg102443;
                    break;
                case "900913":
                    tCoorSys = epsg900913;
                    break;
                default:
                    break;
            }
            return ctf.CreateFromCoordinateSystems(sCoorSys, tCoorSys);
        }
    }
}
