using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

/// <summary>Copies Landing's exact upper frames into the unchanged Build master and refreshes the effects well without its score trough.</summary>
public static class ComposeBuildFrames
{
    /// <summary>Builds the complete framed housing without resizing any artwork or altering the wooden rig and outer shell.</summary>
    public static void Main(string[] args)
    {
        string root=Path.GetFullPath(args.Length>0?args[0]:".");
        string parent=Path.Combine(root,"Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine");
        string folder=Path.Combine(parent,"FramesV2");
        string original=Path.Combine(parent,"integrated-build-machine-source.png");
        string reference=Path.Combine(folder,"Sources/landing-frame-reference.png");
        string generated=Path.Combine(folder,"Sources/bar-removal-generated.png");
        string output=Path.Combine(parent,"integrated-build-machine-v2.png");
        var polygon=new[]{new PointF(220,170),new PointF(1504,170),new PointF(1550,216),new PointF(1550,477),new PointF(1505,529),new PointF(216,529),new PointF(174,483),new PointF(174,215)};
        var bar=new Rectangle(861,414,634,69);
        var portrait=new Rectangle(216,186,614,316);var effects=new Rectangle(844,186,670,316);
        var innerEffects=new[]{new PointF(884,204),new PointF(1470,204),new PointF(1494,227),new PointF(1494,461),new PointF(1472,482),new PointF(884,482),new PointF(863,461),new PointF(863,227)};
        using(var basis=new Bitmap(original))using(var landing=new Bitmap(reference))using(var edit=new Bitmap(generated))using(var path=new GraphicsPath())
        {
            if(basis.Width!=1739||basis.Height!=904||landing.Size!=basis.Size||edit.Width<basis.Width||edit.Height!=basis.Height)
                throw new InvalidOperationException("Frame sources must share the native machine coordinate grid.");
            path.AddPolygon(polygon);
            using(var region=new Region(path))using(var innerPath=new GraphicsPath())
            {
                innerPath.AddPolygon(innerEffects);
                var old=Pixels(basis);var frame=Pixels(landing);var fill=Pixels(edit);var result=(byte[])old.Clone();var mask=new byte[old.Length];var fillMask=new byte[old.Length];
                for(int y=0;y<basis.Height;y++)for(int x=0;x<basis.Width;x++)
                {
                    if(!region.IsVisible(x+.5f,y+.5f))continue;
                    bool useFill=innerPath.IsVisible(x+.5f,y+.5f);
                    int i=(y*basis.Width+x)*4,j=useFill?(y*edit.Width+x)*4:i;
                    var source=useFill?fill:frame;
                    result[i]=source[j];result[i+1]=source[j+1];result[i+2]=source[j+2];
                    mask[i]=mask[i+1]=mask[i+2]=mask[i+3]=255;
                    if(useFill)fillMask[i]=fillMask[i+1]=fillMask[i+2]=fillMask[i+3]=255;
                }
                Save(result,basis.Width,basis.Height,output);Save(mask,basis.Width,basis.Height,Path.Combine(folder,"upper-frame-mask.png"));
                Save(fillMask,basis.Width,basis.Height,Path.Combine(folder,"effects-interior-mask.png"));
                int alphaDifferent=0,protectedDifferent=0,portraitDifferent=0,effectsDifferent=0,barDifferent=0;
                using(var final=new Bitmap(output))
                {
                    var actual=Pixels(final);
                    for(int y=0;y<basis.Height;y++)for(int x=0;x<basis.Width;x++)
                    {
                        int i=(y*basis.Width+x)*4;
                        if(actual[i+3]!=old[i+3])alphaDifferent++;
                        if(mask[i+3]==0&&old[i+3]>0&&Different(actual,old,i))protectedDifferent++;
                        if(portrait.Contains(x,y)&&Different(actual,frame,i))portraitDifferent++;
                        if(effects.Contains(x,y)&&fillMask[i+3]==0&&Different(actual,frame,i))effectsDifferent++;
                        if(bar.Contains(x,y)&&Different(actual,frame,i))barDifferent++;
                    }
                }
                if(alphaDifferent!=0||protectedDifferent!=0||portraitDifferent!=0||effectsDifferent!=0||barDifferent==0)
                    throw new InvalidOperationException("The framed Build variant changed protected artwork or failed to remove the bar.");
                using(var hash=SHA256.Create())
                {
                    var report=new{width=basis.Width,height=basis.Height,upperInterior=polygon,barRemoval=bar,portraitFrame=portrait,effectsFrame=effects,effectsInterior=innerEffects,
                        alphaDifferences=alphaDifferent,protectedVisibleDifferences=protectedDifferent,portraitFrameDifferences=portraitDifferent,effectsBorderDifferences=effectsDifferent,barAreaDifferentPixels=barDifferent,
                        originalSha256=Digest(hash,original),landingReferenceSha256=Digest(hash,reference),outputSha256=Digest(hash,output)};
                    File.WriteAllText(Path.Combine(folder,"CompositionVerification.json"),new JavaScriptSerializer().Serialize(report));
                }
                Console.WriteLine("Build frames: alpha/rig/exterior differences 0; portrait differences 0; effects border differences 0.");
            }
        }
    }

    /// <summary>Compares visible color channels on the native grid.</summary>
    private static bool Different(byte[] a,byte[] b,int i){return a[i]!=b[i]||a[i+1]!=b[i+1]||a[i+2]!=b[i+2];}

    /// <summary>Records immutable source and result provenance.</summary>
    private static string Digest(HashAlgorithm hash,string file){return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();}

    /// <summary>Reads straight native BGRA without resizing the source.</summary>
    private static byte[] Pixels(Bitmap source)
    {
        using(var bitmap=source.Clone(new Rectangle(0,0,source.Width,source.Height),PixelFormat.Format32bppArgb))
        {
            var data=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
            try{var bytes=new byte[bitmap.Width*bitmap.Height*4];Marshal.Copy(data.Scan0,bytes,0,bytes.Length);return bytes;}
            finally{bitmap.UnlockBits(data);}
        }
    }

    /// <summary>Writes the final whole housing or its documented editable-region mask.</summary>
    private static void Save(byte[] bytes,int width,int height,string file)
    {
        using(var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb))
        {
            var data=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
            try{Marshal.Copy(bytes,0,data.Scan0,bytes.Length);}finally{bitmap.UnlockBits(data);}
            bitmap.Save(file,ImageFormat.Png);
        }
    }
}
