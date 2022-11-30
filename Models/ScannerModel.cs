﻿
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using KScanLib;
public sealed partial class ScannerModel
{

    public static object InitiateScanRule(ref ScanRule oScanRule)
    {

        oScanRule.OutputSettingsId = "Default PC Files";
        oScanRule.BlackBorderWhiteNoiseGap = 3;
        oScanRule.DeviceBackRotate =(Int16)KScanLib.enumKGRotate.KGROTATE0;
        oScanRule.DeviceFrontRotate = (Int16)KScanLib.enumKGRotate.KGROTATE0;
        oScanRule.ScanColorMode =           (Int16)KScanLib.enumContants.KSSCANCOLORMODEBITONAL;
        oScanRule.ScanStartTimeOut = 120;
        oScanRule.ActiveDevice =            (Int16)KScanLib.enumActiveDevice.KSACTIVEDEVICESCANNER;
        oScanRule.DeviceCache =             (Int16)KScanLib.enumDeviceCache.KSCACHENONE;
        oScanRule.DeviceTimeout = 120;
        oScanRule.Display = true;
        oScanRule.IOCompression =           (Int16)KScanLib.enumKGContants.KGCOMPG4;
        oScanRule.IOStgFlt = "TIFF";
        oScanRule.ScanContrast =            (Int16)KScanLib.enumKGContants.KGCONTRASTAUTO;
        oScanRule.ScanDensity = (Int16)KScanLib.enumKGContants.KGDENSITYAUTO;
        oScanRule.ScanDestination = (Int16)KScanLib.enumContants.KSDESTBIN1;
        oScanRule.ScanDirection = (Int16)KScanLib.enumContants.KSSCANDIRECTPORTRAIT;
        oScanRule.ScanDPI = (Int16)KScanLib.enumKGContants.KGDPI300;
        oScanRule.ScanMode = (Int16)KScanLib.enumContants.KSSCANMODELINE;
        oScanRule.ScanSource = (Int16)KScanLib.enumContants.KSSOURCEMANUAL;
        oScanRule.DeshadeHorzSpeckleAdj = 1;
        oScanRule.DeshadeHorzSpeckleMax = 12;
        oScanRule.DeshadeMinHeight = 20;
        oScanRule.DeshadeMinWidth = 100;
        oScanRule.DeshadeUnit = (Int16)KScanLib.enumKGUnits.KGUNITPIXELS;
        oScanRule.DeshadeVertSpeckleAdj = 1;
        oScanRule.DeshadeVertSpeckleMax = 12;
        oScanRule.DespeckleHeight = 4;
        oScanRule.DespeckleUnit = (Int16)KScanLib.enumKGUnits.KGUNITPIXELS;
        oScanRule.DespeckleWidth = 4;
        oScanRule.EdgeEnhancementAlgorithm = (Int16)KimgpLib.enumContants.KIEDGECHARSMOOTHING;
        oScanRule.HorzLineEdgeCleanFactor = 2;
        oScanRule.HorzLineMaxGap = 2;
        oScanRule.HorzLineMaxHeight = 20;
        oScanRule.HorzLineMinLength = 200;
        oScanRule.HorzLineReconstructionHeight = 4;
        oScanRule.HorzLineReconstructionWidth = 40;
        oScanRule.HorzLineUnit = (Int16)KScanLib.enumKGUnits.KGUNITPIXELS;
        oScanRule.PatchTriggers = (Int16)KimgpLib.enumContants.KIPATCHNONE;
        oScanRule.StreakWidth = 1;
        oScanRule.VertLineEdgeCleanFactor = 2;
        oScanRule.VertLineMaxGap = 2;
        oScanRule.VertLineMaxWidth = 20;
        oScanRule.VertLineMinHeight = 200;
        oScanRule.VertLineReconstructionHeight = 4;
        oScanRule.VertLineReconstructionWidth = 40;
        oScanRule.VertLineUnit = (Int16)KScanLib.enumKGUnits.KGUNITPIXELS;
        oScanRule.BarHeight = 0.5d;
        oScanRule.BarHorzMax = 1;
        oScanRule.BarMax = 5;
        oScanRule.BarOrientation = (Int16)KimgpLib.enumContants.KIBARORIENTATION0 | (Int16)KimgpLib.enumContants.KIBARORIENTATION90 | (Int16)KimgpLib.enumContants.KIBARORIENTATION180 | (Int16)KimgpLib.enumContants.KIBARORIENTATION270;
        oScanRule.BarQuality = (Int16)KimgpLib.enumBarQuality.KIBARQUALITYGOOD;
        oScanRule.BarRatio = true;
        oScanRule.BarReturnsPatch = (Int16)KimgpLib.enumContants.KIPATCHNONE;
        oScanRule.BarType = (Int16)KimgpLib.enumContants.KIBARTYPE3OF9;
        oScanRule.BarWidth = 0.014d;
        oScanRule.FontBackGround = (Int16)KimgpLib.enumFontBackground.KIFONTBACKGROUNDTRANSPARENT;
        oScanRule.FontDPI = (Int16)KimgpLib.enumFontDpi.KIFONTDPI200;
        oScanRule.FontName = (Int16)KimgpLib.enumFontName.KIFONTNAMESANSSERIF;
        oScanRule.FontOrientation = (Int16)KimgpLib.enumFontOrientation.KIFONTORIENTATIONHORZ0;
        oScanRule.FontSize = (Int16)KimgpLib.enumFontSize.KIFONTSIZE8;
        oScanRule.PatchLeft = 6;
        oScanRule.SkewMaxAngle = (Int16)KimgpLib.enumContants.KISKEWMAXANGLE;
        oScanRule.SkewMinAngle = (Int16)KimgpLib.enumContants.KISKEWMINANGLE;

        oScanRule.AsyncInterval = 0;
        oScanRule.DeviceDeleteBack = 0;
        oScanRule.DeviceDeleteFront = 0;
        oScanRule.Unit = 0;
        oScanRule.BarDensity = 0;
        oScanRule.BarLength = 0;
        oScanRule.BarSkew = 0;
        oScanRule.PSAnnotateLeft = 0;
        oScanRule.PSAnnotateTop = 0;
        oScanRule.PSEndorseLeft = 0;
        oScanRule.PSEndorseTop = 0;
        oScanRule.DocumentChangeRule = 0;
        oScanRule.ViewGroup = 0;
        oScanRule.BlackBorder = 0;
        oScanRule.DeshadeCorrect = 0;
        oScanRule.DeshadeDetect = 0;
        oScanRule.Despeckle = 0;
        oScanRule.DeviceBackPickingHeight = 0;
        oScanRule.DeviceBackPickingLeft = 0;
        oScanRule.DeviceBackPickingTop = 0;
        oScanRule.DeviceBackPickingWidth = 0;
        oScanRule.DeviceFrontPickingHeight = 0;
        oScanRule.DeviceFrontPickingLeft = 0;
        oScanRule.DeviceFrontPickingTop = 0;
        oScanRule.DeviceFrontPickingWidth = 0;
        oScanRule.EdgeEnhancement = 0;
        oScanRule.HorzLineReconstruct = 0;
        oScanRule.HorzLineRemoval = 0;
        oScanRule.MultiPage = 0;
        oScanRule.PatchTrigger = 0;
        oScanRule.ScanBackSize = 0;
        oScanRule.ScanDither = 0;
        oScanRule.ScanFrontSize = 0;
        oScanRule.SliceCols = 0;
        oScanRule.SliceHeight = 0;
        oScanRule.SliceOffsetCol = 0;
        oScanRule.SliceOffsetRow = 0;
        oScanRule.SliceOffsetX = 0;
        oScanRule.SliceOffsetY = 0;
        oScanRule.SliceRows = 0;
        oScanRule.SliceSkipBarcodes = 0;
        oScanRule.SliceWidth = 0;
        oScanRule.StreakRemoval = 0;
        oScanRule.VertLineReconstruct = 0;
        oScanRule.VertLineRemoval = 0;
        oScanRule.MultiPageWriteWhenComplete =false;

        return oScanRule;
    }

    public static void InitiateCloneScanRule(ref ScanRule cloneScanRule, ScanRule oScanRule)
    {

        cloneScanRule.OutputSettingsId = oScanRule.OutputSettingsId;
        cloneScanRule.BlackBorderWhiteNoiseGap = oScanRule.BlackBorderWhiteNoiseGap;
        cloneScanRule.DeviceBackRotate = oScanRule.DeviceBackRotate;
        cloneScanRule.DeviceFrontRotate = oScanRule.DeviceFrontRotate;
        cloneScanRule.ScanColorMode = oScanRule.ScanColorMode;
        cloneScanRule.ScanStartTimeOut = oScanRule.ScanStartTimeOut;
        cloneScanRule.ActiveDevice = oScanRule.ActiveDevice;
        cloneScanRule.DeviceCache = oScanRule.DeviceCache;
        cloneScanRule.DeviceTimeout = oScanRule.DeviceTimeout;
        cloneScanRule.Display = oScanRule.Display;
        cloneScanRule.IOCompression = oScanRule.IOCompression;
        cloneScanRule.IOStgFlt = oScanRule.IOStgFlt;
        cloneScanRule.ScanContrast = oScanRule.ScanContrast;
        cloneScanRule.ScanDensity = oScanRule.ScanDensity;
        cloneScanRule.ScanDestination = oScanRule.ScanDestination;
        cloneScanRule.ScanDirection = oScanRule.ScanDirection;
        cloneScanRule.ScanDPI = oScanRule.ScanDPI;
        cloneScanRule.ScanMode = oScanRule.ScanMode;
        cloneScanRule.ScanSource = oScanRule.ScanSource;
        cloneScanRule.DeshadeHorzSpeckleAdj = oScanRule.DeshadeHorzSpeckleAdj;
        cloneScanRule.DeshadeHorzSpeckleMax = oScanRule.DeshadeHorzSpeckleMax;
        cloneScanRule.DeshadeMinHeight = oScanRule.DeshadeMinHeight;
        cloneScanRule.DeshadeMinWidth = oScanRule.DeshadeMinWidth;
        cloneScanRule.DeshadeUnit = oScanRule.DeshadeUnit;
        cloneScanRule.DeshadeVertSpeckleAdj = oScanRule.DeshadeVertSpeckleAdj;
        cloneScanRule.DeshadeVertSpeckleMax = oScanRule.DeshadeVertSpeckleMax;
        cloneScanRule.DespeckleHeight = oScanRule.DespeckleHeight;
        cloneScanRule.DespeckleUnit = oScanRule.DespeckleUnit;
        cloneScanRule.DespeckleWidth = oScanRule.DespeckleWidth;
        cloneScanRule.EdgeEnhancementAlgorithm = oScanRule.EdgeEnhancementAlgorithm;
        cloneScanRule.HorzLineEdgeCleanFactor = oScanRule.HorzLineEdgeCleanFactor;
        cloneScanRule.HorzLineMaxGap = oScanRule.HorzLineMaxGap;
        cloneScanRule.HorzLineMaxHeight = oScanRule.HorzLineMaxHeight;
        cloneScanRule.HorzLineMinLength = oScanRule.HorzLineMinLength;
        cloneScanRule.HorzLineReconstructionHeight = oScanRule.HorzLineReconstructionHeight;
        cloneScanRule.HorzLineReconstructionWidth = oScanRule.HorzLineReconstructionWidth;
        cloneScanRule.HorzLineUnit = oScanRule.HorzLineUnit;
        cloneScanRule.PatchTriggers = oScanRule.PatchTriggers;
        cloneScanRule.StreakWidth = oScanRule.StreakWidth;
        cloneScanRule.VertLineEdgeCleanFactor = oScanRule.VertLineEdgeCleanFactor;
        cloneScanRule.VertLineMaxGap = oScanRule.VertLineMaxGap;
        cloneScanRule.VertLineMaxWidth = oScanRule.VertLineMaxWidth;
        cloneScanRule.VertLineMinHeight = oScanRule.VertLineMinHeight;
        cloneScanRule.VertLineReconstructionHeight = oScanRule.VertLineReconstructionHeight;
        cloneScanRule.VertLineReconstructionWidth = oScanRule.VertLineReconstructionWidth;
        cloneScanRule.VertLineUnit = oScanRule.VertLineUnit;
        cloneScanRule.BarHeight = oScanRule.BarHeight;
        cloneScanRule.BarHorzMax = oScanRule.BarHorzMax;
        cloneScanRule.BarMax = oScanRule.BarMax;
        cloneScanRule.BarOrientation = oScanRule.BarOrientation;
        cloneScanRule.BarQuality = oScanRule.BarQuality;
        cloneScanRule.BarRatio = oScanRule.BarRatio;
        cloneScanRule.BarReturnsPatch = oScanRule.BarReturnsPatch;
        cloneScanRule.BarType = oScanRule.BarType;
        cloneScanRule.BarWidth = oScanRule.BarWidth;
        cloneScanRule.FontBackGround = oScanRule.FontBackGround;
        cloneScanRule.FontDPI = oScanRule.FontDPI;
        cloneScanRule.FontName = oScanRule.FontName;
        cloneScanRule.FontOrientation = oScanRule.FontOrientation;
        cloneScanRule.FontSize = oScanRule.FontSize;
        cloneScanRule.PatchLeft = oScanRule.PatchLeft;
        cloneScanRule.SkewMaxAngle = oScanRule.SkewMaxAngle;
        cloneScanRule.SkewMinAngle = oScanRule.SkewMinAngle;

        cloneScanRule.AsyncInterval = oScanRule.AsyncInterval;
        cloneScanRule.DeviceDeleteBack = oScanRule.DeviceDeleteBack;
        cloneScanRule.DeviceDeleteFront = oScanRule.DeviceDeleteFront;
        cloneScanRule.Unit = oScanRule.Unit;
        cloneScanRule.BarDensity = oScanRule.BarDensity;
        cloneScanRule.BarLength = oScanRule.BarLength;
        cloneScanRule.BarSkew = oScanRule.BarSkew;
        cloneScanRule.PSAnnotateLeft = oScanRule.PSAnnotateLeft;
        cloneScanRule.PSAnnotateTop = oScanRule.PSAnnotateTop;
        cloneScanRule.PSEndorseLeft = oScanRule.PSEndorseLeft;
        cloneScanRule.PSEndorseTop = oScanRule.PSEndorseTop;
        cloneScanRule.DocumentChangeRule = oScanRule.DocumentChangeRule;
        cloneScanRule.ViewGroup = oScanRule.ViewGroup;
        cloneScanRule.BlackBorder = oScanRule.BlackBorder;
        cloneScanRule.DeshadeCorrect = oScanRule.DeshadeCorrect;
        cloneScanRule.DeshadeDetect = oScanRule.DeshadeDetect;
        cloneScanRule.Despeckle = oScanRule.Despeckle;
        cloneScanRule.DeviceBackPickingHeight = oScanRule.DeviceBackPickingHeight;
        cloneScanRule.DeviceBackPickingLeft = oScanRule.DeviceBackPickingLeft;
        cloneScanRule.DeviceBackPickingTop = oScanRule.DeviceBackPickingTop;
        cloneScanRule.DeviceBackPickingWidth = oScanRule.DeviceBackPickingWidth;
        cloneScanRule.DeviceFrontPickingHeight = oScanRule.DeviceFrontPickingHeight;
        cloneScanRule.DeviceFrontPickingLeft = oScanRule.DeviceFrontPickingLeft;
        cloneScanRule.DeviceFrontPickingTop = oScanRule.DeviceFrontPickingTop;
        cloneScanRule.DeviceFrontPickingWidth = oScanRule.DeviceFrontPickingWidth;
        cloneScanRule.EdgeEnhancement = oScanRule.EdgeEnhancement;
        cloneScanRule.HorzLineReconstruct = oScanRule.HorzLineReconstruct;
        cloneScanRule.HorzLineRemoval = oScanRule.HorzLineRemoval;
        cloneScanRule.MultiPage = oScanRule.MultiPage;
        cloneScanRule.PatchTrigger = oScanRule.PatchTrigger;
        cloneScanRule.ScanBackSize = oScanRule.ScanBackSize;
        cloneScanRule.ScanDither = oScanRule.ScanDither;
        cloneScanRule.ScanFrontSize = oScanRule.ScanFrontSize;
        cloneScanRule.SliceCols = oScanRule.SliceCols;
        cloneScanRule.SliceHeight = oScanRule.SliceHeight;
        cloneScanRule.SliceOffsetCol = oScanRule.SliceOffsetCol;
        cloneScanRule.SliceOffsetRow = oScanRule.SliceOffsetRow;
        cloneScanRule.SliceOffsetX = oScanRule.SliceOffsetX;
        cloneScanRule.SliceOffsetY = oScanRule.SliceOffsetY;
        cloneScanRule.SliceRows = oScanRule.SliceRows;
        cloneScanRule.SliceSkipBarcodes = oScanRule.SliceSkipBarcodes;
        cloneScanRule.SliceWidth = oScanRule.SliceWidth;
        cloneScanRule.StreakRemoval = oScanRule.StreakRemoval;
        cloneScanRule.VertLineReconstruct = oScanRule.VertLineReconstruct;
        cloneScanRule.VertLineRemoval = oScanRule.VertLineRemoval;
        cloneScanRule.MultiPageWriteWhenComplete = oScanRule.MultiPageWriteWhenComplete;

    }


}