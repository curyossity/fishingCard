using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

/// <summary>Registers the generated Discover interior to the original Build pixels without changing any exterior pixel or alpha value.</summary>
public static class NormalizeDiscoverHousing
{
    /// <summary>Copies generated colors only inside the two native working recesses, retaining the complete original shell and silhouette.</summary>
    public static void Main(string[] args)
    {
        string root=Path.GetFullPath(args.Length>0?args[0]:".");
        string folder=Path.Combine(root,"Docs/UIThemes/TackleApparatusConcept/DiscoverIntegrated");
        string original=Path.Combine(root,"Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine/integrated-build-machine-source.png");
        string generated=Path.Combine(folder,"Sources/discover-generated.png");
        string output=Path.Combine(folder,"discover-machine.png");
        var top=new[]{new PointF(220,170),new PointF(1504,170),new PointF(1550,216),new PointF(1550,477),new PointF(1505,529),new PointF(216,529),new PointF(174,483),new PointF(174,215)};
        var bottom=new[]{new PointF(215,569),new PointF(1494,569),new PointF(1520,598),new PointF(1520,812),new PointF(1490,840),new PointF(211,840),new PointF(176,808),new PointF(176,600)};
        using(var basis=new Bitmap(original))using(var candidate=new Bitmap(generated))using(var path=new GraphicsPath())
        {
            if(basis.Width!=1739||basis.Height!=904||candidate.Width<basis.Width||candidate.Height!=basis.Height)
                throw new InvalidOperationException("The generated interior must retain the native Build coordinate grid.");
            path.AddPolygon(top);path.AddPolygon(bottom);
            using(var region=new Region(path))
            {
                byte[] shell=Pixels(basis),inside=Pixels(candidate),result=(byte[])shell.Clone(),mask=new byte[shell.Length];
                int edited=0;
                for(int y=0;y<basis.Height;y++)for(int x=0;x<basis.Width;x++)
                {
                    if(!region.IsVisible(x+.5f,y+.5f))continue;
                    int target=(y*basis.Width+x)*4,source=(y*candidate.Width+x)*4;
                    result[target]=inside[source];result[target+1]=inside[source+1];result[target+2]=inside[source+2];
                    // The alpha is always inherited from the shell, including inside the editable recesses.
                    mask[target]=mask[target+1]=mask[target+2]=mask[target+3]=255;edited++;
                }
                Save(result,basis.Width,basis.Height,output);
                Save(mask,basis.Width,basis.Height,Path.Combine(folder,"discover-interior-mask.png"));
                int alphaChanged=0,exteriorChanged=0;
                using(var normalized=new Bitmap(output))
                {
                    var actual=Pixels(normalized);
                    for(int i=0;i<shell.Length;i+=4)
                    {
                        if(shell[i+3]!=actual[i+3])alphaChanged++;
                        if(mask[i+3]==0&&shell[i+3]>0&&(shell[i]!=actual[i]||shell[i+1]!=actual[i+1]||shell[i+2]!=actual[i+2]))exteriorChanged++;
                    }
                }
                if(alphaChanged!=0||exteriorChanged!=0)throw new InvalidOperationException("Normalization changed the protected shell.");
                using(var hash=SHA256.Create())
                {
                    var report=new {width=basis.Width,height=basis.Height,generatedWidth=candidate.Width,generatedHeight=candidate.Height,
                        editablePixels=edited,protectedPixels=basis.Width*basis.Height-edited,alphaDifferences=alphaChanged,protectedVisibleColorDifferences=exteriorChanged,
                        baseSha256=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(original))).Replace("-","").ToLowerInvariant(),
                        outputSha256=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(output))).Replace("-","").ToLowerInvariant(),
                        topInterior=top,bottomInterior=bottom,registration="Native 1:1 interior coordinates; original Build alpha and exterior retained."};
                    File.WriteAllText(Path.Combine(folder,"Normalization.json"),new JavaScriptSerializer().Serialize(report));
                }
                Console.WriteLine("Locked shell: "+(basis.Width*basis.Height-edited)+" protected pixels; alpha differences: "+alphaChanged+"; visible exterior differences: "+exteriorChanged);
            }
        }
    }

    /// <summary>Reads straight BGRA pixels without resizing or interpolating the source.</summary>
    private static byte[] Pixels(Bitmap source)
    {
        using(var bitmap=source.Clone(new Rectangle(0,0,source.Width,source.Height),PixelFormat.Format32bppArgb))
        {
            var data=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
            try{var bytes=new byte[bitmap.Width*bitmap.Height*4];Marshal.Copy(data.Scan0,bytes,0,bytes.Length);return bytes;}
            finally{bitmap.UnlockBits(data);}
        }
    }

    /// <summary>Writes the normalized native-size RGBA delivery or its documented interior mask.</summary>
    private static void Save(byte[] bytes,int width,int height,string path)
    {
        using(var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb))
        {
            var data=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
            try{Marshal.Copy(bytes,0,data.Scan0,bytes.Length);}finally{bitmap.UnlockBits(data);}
            bitmap.Save(path,ImageFormat.Png);
        }
    }
}
