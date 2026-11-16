using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
public class ConceptCommand { public string kind,id,src,text,font,color,align; public float x,y,w,h,sx,sy,sw,sh,size; public float[] mask; public bool shadow; }
public class ConceptScene { public string name,file; public int width,height; public ConceptCommand[] commands; }
public static class ConceptRenderer {
static string N(float v){return v.ToString("0.###",CultureInfo.InvariantCulture);}
static string E(string s){return System.Security.SecurityElement.Escape(s??"");}
static string PathData(GraphicsPath path){var p=path.PathPoints;var t=path.PathTypes;var s=new StringBuilder();for(int i=0;i<p.Length;){int type=t[i]&7;if(type==0){s.Append("M"+N(p[i].X)+","+N(p[i].Y));if((t[i]&128)!=0)s.Append("Z");i++;}else if(type==1){s.Append("L"+N(p[i].X)+","+N(p[i].Y));if((t[i]&128)!=0)s.Append("Z");i++;}else if(type==3){s.Append("C"+N(p[i].X)+","+N(p[i].Y)+" "+N(p[i+1].X)+","+N(p[i+1].Y)+" "+N(p[i+2].X)+","+N(p[i+2].Y));if((t[i+2]&128)!=0)s.Append("Z");i+=3;}else throw new Exception("Unsupported font path");}return s.ToString();}
public static void Render(string folder,string fontsFolder){
var json=new JavaScriptSerializer{MaxJsonLength=100000000};var scenes=json.Deserialize<ConceptScene[]>(File.ReadAllText(Path.Combine(folder,"Composition.json")));
using(var fonts=new PrivateFontCollection()){
fonts.AddFontFile(Path.Combine(fontsFolder,"PressStart2P-Regular.ttf"));fonts.AddFontFile(Path.Combine(fontsFolder,"VT323-Regular.ttf"));
foreach(var scene in scenes){var images=new Dictionary<string,Image>();var ids=new Dictionary<string,string>();var defs=new StringBuilder();var content=new StringBuilder();var textReport=new List<object>();
using(var bitmap=new Bitmap(scene.width,scene.height,PixelFormat.Format32bppArgb))using(var graphics=Graphics.FromImage(bitmap)){
graphics.Clear(Color.FromArgb(14,17,15));graphics.InterpolationMode=InterpolationMode.NearestNeighbor;graphics.PixelOffsetMode=PixelOffsetMode.Half;graphics.SmoothingMode=SmoothingMode.None;
int counter=0;foreach(var c in scene.commands){counter++;content.Append("<g id=\""+E(c.id)+"-"+counter+"\">");
if(c.kind=="image"){
if(!images.ContainsKey(c.src))images[c.src]=Image.FromFile(c.src);
string encoded;
using(var cropped=new Bitmap((int)c.sw,(int)c.sh,PixelFormat.Format32bppArgb)){
using(var crop=Graphics.FromImage(cropped)){crop.CompositingMode=CompositingMode.SourceCopy;
if(c.mask!=null){using(var mask=new GraphicsPath()){var points=new PointF[c.mask.Length/2];for(int i=0;i<points.Length;i++)points[i]=new PointF(c.mask[i*2],c.mask[i*2+1]);mask.AddPolygon(points);crop.SetClip(mask);}}
crop.DrawImage(images[c.src],new Rectangle(0,0,(int)c.sw,(int)c.sh),new Rectangle((int)c.sx,(int)c.sy,(int)c.sw,(int)c.sh),GraphicsUnit.Pixel);}
graphics.DrawImage(cropped,new RectangleF(c.x,c.y,c.w,c.h),new RectangleF(0,0,c.sw,c.sh),GraphicsUnit.Pixel);
using(var stream=new MemoryStream()){cropped.Save(stream,ImageFormat.Png);encoded=Convert.ToBase64String(stream.ToArray());}}
content.Append("<image x=\""+N(c.x)+"\" y=\""+N(c.y)+"\" width=\""+N(c.w)+"\" height=\""+N(c.h)+"\" preserveAspectRatio=\"none\" xlink:href=\"data:image/png;base64,"+encoded+"\"/>");
}else if(c.kind=="text"){
var family=fonts.Families.First(f=>c.font=="body"?f.Name.Contains("VT323"):!f.Name.Contains("VT323"));
using(var path=new GraphicsPath()){
path.AddString(c.text,family,(int)FontStyle.Regular,c.size,new PointF(0,0),StringFormat.GenericTypographic);var bounds=path.GetBounds();float scale=Math.Min(1,c.w/Math.Max(1,bounds.Width));
using(var matrix=new Matrix()){matrix.Scale(scale,scale);path.Transform(matrix);}bounds=path.GetBounds();float x=c.align=="left"?c.x:c.x+(c.w-bounds.Width)/2;
using(var matrix=new Matrix()){matrix.Translate(x-bounds.X,c.y-bounds.Y);path.Transform(matrix);}bounds=path.GetBounds();
if(bounds.Left<0||bounds.Right>scene.width+.5||bounds.Top<0||bounds.Bottom>scene.height+.5)throw new Exception("Label outside screen: "+c.id);
string d=PathData(path);content.Append("<title>"+E(c.text)+"</title>");
if(c.shadow){using(var shadow=(GraphicsPath)path.Clone()){using(var matrix=new Matrix()){matrix.Translate(2,2);shadow.Transform(matrix);}using(var brush=new SolidBrush(Color.FromArgb(20,23,20)))graphics.FillPath(brush,shadow);}content.Append("<path d=\""+d+"\" fill=\"#141714\" transform=\"translate(2 2)\"/>");}
using(var brush=new SolidBrush(ColorTranslator.FromHtml(c.color)))graphics.FillPath(brush,path);
content.Append("<path d=\""+d+"\" fill=\""+c.color+"\"/>");textReport.Add(new {id=c.id,text=c.text,font=family.Name,x=bounds.X,y=bounds.Y,width=bounds.Width,height=bounds.Height});
}}
content.Append("</g>");}
bitmap.Save(Path.Combine(folder,scene.file+".png"),ImageFormat.Png);
File.WriteAllText(Path.Combine(folder,scene.file+".svg"),"<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" width=\""+scene.width+"\" height=\""+scene.height+"\" viewBox=\"0 0 "+scene.width+" "+scene.height+"\"><title>"+E(scene.name)+"</title><defs>"+defs+"</defs>"+content+"</svg>",new UTF8Encoding(false));
File.WriteAllText(Path.Combine(folder,scene.file+"-Text.json"),json.Serialize(textReport));
}foreach(var image in images.Values)image.Dispose();
}}
}
}
public static class ConceptProgram { public static void Main(string[] args) { ConceptRenderer.Render(args[0],args[1]); } }
