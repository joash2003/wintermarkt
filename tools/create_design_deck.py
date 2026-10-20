#!/usr/bin/env python3
"""Create the editable, self-contained first design-document deck with native PowerPoint shapes."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from xml.etree import ElementTree as ET
from xml.sax.saxutils import escape
import re

OUT = Path(__file__).resolve().parents[1] / 'docs/submission/Linz_Wintermarkt_Initial_Design_Document_v0.1.pptx'
W, H = 13.333333, 7.5
EMU = 914400
LOGO = Path(__file__).resolve().parents[1] / 'docs/submission/assets/JKU_Logo.png'

# Palette: paper, deep evergreen, berry, warm gold, ice, and slate.
C = {
    'paper':'F7F5EF', 'ink':'172C2B', 'pine':'173C36', 'pine2':'28564D',
    'berry':'A63D42', 'gold':'E6B75A', 'cream':'FFFDF7', 'ice':'DCE9E8',
    'mist':'EEF1ED', 'slate':'667775', 'muted':'9AABA5', 'white':'FFFFFF',
    'road':'374441', 'path':'D8CDB7', 'tree':'456D59', 'redlight':'F3C5A8',
    'green':'64A17B', 'red':'C76863', 'blue':'4F8992', 'orange':'CE7548'
}
FONT = 'Arial'
slides=[]
_ROOTS=[]

def e(tag, attrs=None, parent=None):
    x=ET.Element(tag, attrs or {})
    if parent is not None: parent.append(x)
    return x

def emu(v): return str(round(v*EMU))

def base_slide(bg='paper'):
    root=ET.Element('{http://schemas.openxmlformats.org/presentationml/2006/main}sld', {'showMasterSp':'1'})
    root.set('{http://www.w3.org/XML/1998/namespace}space','preserve')
    cS=ET.SubElement(root,'{http://schemas.openxmlformats.org/presentationml/2006/main}cSld')
    spTree=ET.SubElement(cS,'{http://schemas.openxmlformats.org/presentationml/2006/main}spTree')
    nv=ET.SubElement(spTree,'{http://schemas.openxmlformats.org/presentationml/2006/main}nvGrpSpPr')
    ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}cNvPr',{'id':'1','name':''})
    ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}cNvGrpSpPr')
    ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}nvPr')
    grp=ET.SubElement(spTree,'{http://schemas.openxmlformats.org/presentationml/2006/main}grpSpPr')
    xf=ET.SubElement(grp,'{http://schemas.openxmlformats.org/drawingml/2006/main}xfrm')
    ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}off',{'x':'0','y':'0'}); ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}ext',{'cx':'0','cy':'0'}); ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}chOff',{'x':'0','y':'0'}); ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}chExt',{'cx':'0','cy':'0'})
    s=Slide(spTree)
    s.rect(0,0,W,H,C[bg])
    ET.SubElement(root,'{http://schemas.openxmlformats.org/presentationml/2006/main}clrMapOvr').append(ET.Element('{http://schemas.openxmlformats.org/drawingml/2006/main}masterClrMapping'))
    _ROOTS.append(root)
    return s

class Slide:
    def __init__(self, tree): self.tree=tree; self.i=2
    def _shape(self,name,x,y,w,h,geom='rect',fill=None,line=None,trans=0,rot=0):
        p='http://schemas.openxmlformats.org/presentationml/2006/main'; a='http://schemas.openxmlformats.org/drawingml/2006/main'
        sp=ET.SubElement(self.tree,f'{{{p}}}sp')
        nv=ET.SubElement(sp,f'{{{p}}}nvSpPr'); ET.SubElement(nv,f'{{{p}}}cNvPr',{'id':str(self.i),'name':name}); self.i+=1
        ET.SubElement(nv,f'{{{p}}}cNvSpPr'); ET.SubElement(nv,f'{{{p}}}nvPr')
        pr=ET.SubElement(sp,f'{{{p}}}spPr'); xf=ET.SubElement(pr,f'{{{a}}}xfrm',{'rot':str(rot*60000)})
        ET.SubElement(xf,f'{{{a}}}off',{'x':emu(x),'y':emu(y)}); ET.SubElement(xf,f'{{{a}}}ext',{'cx':emu(w),'cy':emu(h)})
        ET.SubElement(pr,f'{{{a}}}prstGeom',{'prst':geom}).append(ET.Element(f'{{{a}}}avLst') )
        if fill:
            sf=ET.SubElement(pr,f'{{{a}}}solidFill'); ET.SubElement(sf,f'{{{a}}}srgbClr',{'val':C.get(fill,fill)})
            if trans: ET.SubElement(sf[0],f'{{{a}}}alpha',{'val':str(round((1-trans)*100000))})
        else: ET.SubElement(pr,f'{{{a}}}noFill')
        if line:
            ln=ET.SubElement(pr,f'{{{a}}}ln',{'w':'12700'}); ET.SubElement(ln,f'{{{a}}}solidFill').append(ET.Element(f'{{{a}}}srgbClr',{'val':C.get(line,line)}))
        else: ET.SubElement(pr,f'{{{a}}}ln').append(ET.Element(f'{{{a}}}noFill'))
        return sp
    def rect(self,x,y,w,h,fill='cream',radius=False,line=None,trans=0,name='shape'):
        return self._shape(name,x,y,w,h,'roundRect' if radius else 'rect',fill,line,trans)
    def oval(self,x,y,w,h,fill='gold',line=None,name='dot'):
        return self._shape(name,x,y,w,h,'ellipse',fill,line)
    def poly(self,pts,fill,line=None,name='facet'):
        # Freeform polygon; the custom path replaces the preset geometry.
        a='http://schemas.openxmlformats.org/drawingml/2006/main'; p='http://schemas.openxmlformats.org/presentationml/2006/main'
        x0=min(q[0] for q in pts); y0=min(q[1] for q in pts)
        w=max(.001,max(q[0] for q in pts)-x0); h=max(.001,max(q[1] for q in pts)-y0)
        sp=self._shape(name,x0,y0,w,h,'rect',fill,line)
        pr=sp.find(f'{{{p}}}spPr'); geom=pr.find(f'{{{a}}}prstGeom'); i=list(pr).index(geom); pr.remove(geom)
        cg=ET.Element(f'{{{a}}}custGeom')
        for tag in ('avLst','gdLst','ahLst','cxnLst'): ET.SubElement(cg,f'{{{a}}}{tag}')
        ET.SubElement(cg,f'{{{a}}}rect',{'l':'0','t':'0','r':'r','b':'b'})
        path=ET.SubElement(ET.SubElement(cg,f'{{{a}}}pathLst'),f'{{{a}}}path',{'w':emu(w),'h':emu(h)})
        for k,(px,py) in enumerate(pts):
            ET.SubElement(ET.SubElement(path,f'{{{a}}}{"moveTo" if k==0 else "lnTo"}'),f'{{{a}}}pt',{'x':emu(px-x0),'y':emu(py-y0)})
        ET.SubElement(path,f'{{{a}}}close')
        pr.insert(i,cg)
        return sp
    def line(self,x1,y1,x2,y2,color='muted',width=1.3,arrow=False):
        a='http://schemas.openxmlformats.org/drawingml/2006/main'; p='http://schemas.openxmlformats.org/presentationml/2006/main'
        x=min(x1,x2); y=min(y1,y2); w=max(.001,abs(x2-x1)); h=max(.001,abs(y2-y1)); rot=0
        # DrawingML line shape naturally connects top-left to bottom-right; flip for negative slope.
        sp=self._shape('connector',x,y,w,h,'line',None,color)
        pr=sp.find(f'{{{p}}}spPr'); xf=pr.find(f'{{{a}}}xfrm')
        if (x2-x1)*(y2-y1)<0: xf.set('flipV','1')
        ln=pr.find(f'{{{a}}}ln'); ln.set('w',str(round(width*12700)))
        if arrow: ET.SubElement(ln,f'{{{a}}}headEnd',{'type':'none'}); ET.SubElement(ln,f'{{{a}}}tailEnd',{'type':'triangle','w':'med','len':'med'})
    def text(self,x,y,w,h,value,size=16,color='ink',bold=False,align='l',valign='ctr',font=FONT,margin=.02,italic=False):
        p='http://schemas.openxmlformats.org/presentationml/2006/main'; a='http://schemas.openxmlformats.org/drawingml/2006/main'
        sp=ET.SubElement(self.tree,f'{{{p}}}sp'); nv=ET.SubElement(sp,f'{{{p}}}nvSpPr')
        ET.SubElement(nv,f'{{{p}}}cNvPr',{'id':str(self.i),'name':'Text'}); self.i+=1
        ET.SubElement(nv,f'{{{p}}}cNvSpPr',{'txBox':'1'}); ET.SubElement(nv,f'{{{p}}}nvPr')
        pr=ET.SubElement(sp,f'{{{p}}}spPr'); xf=ET.SubElement(pr,f'{{{a}}}xfrm'); ET.SubElement(xf,f'{{{a}}}off',{'x':emu(x),'y':emu(y)}); ET.SubElement(xf,f'{{{a}}}ext',{'cx':emu(w),'cy':emu(h)})
        ET.SubElement(pr,f'{{{a}}}prstGeom',{'prst':'rect'}).append(ET.Element(f'{{{a}}}avLst')); ET.SubElement(pr,f'{{{a}}}noFill'); ET.SubElement(pr,f'{{{a}}}ln').append(ET.Element(f'{{{a}}}noFill'))
        tx=ET.SubElement(sp,f'{{{p}}}txBody'); ET.SubElement(tx,f'{{{a}}}bodyPr',{'wrap':'square','lIns':emu(margin),'rIns':emu(margin),'tIns':emu(margin),'bIns':emu(margin),'anchor':{'t':'t','ctr':'ctr','b':'b'}[valign]}); ET.SubElement(tx,f'{{{a}}}lstStyle')
        for idx,ln in enumerate(str(value).split('\n')):
            para=ET.SubElement(tx,f'{{{a}}}p'); ET.SubElement(para,f'{{{a}}}pPr',{'algn':{'l':'l','c':'ctr','r':'r'}[align]})
            run=ET.SubElement(para,f'{{{a}}}r'); rpr=ET.SubElement(run,f'{{{a}}}rPr',{'lang':'en-US','sz':str(round(size*100)),'b':'1' if bold else '0','i':'1' if italic else '0'})
            ET.SubElement(rpr,f'{{{a}}}solidFill').append(ET.Element(f'{{{a}}}srgbClr',{'val':C.get(color,color)})); ET.SubElement(rpr,f'{{{a}}}latin',{'typeface':font}); ET.SubElement(run,f'{{{a}}}t').text=ln
            end=ET.SubElement(para,f'{{{a}}}endParaRPr',{'lang':'en-US','sz':str(round(size*100))})
        return sp
    def label(self,x,y,w,h,t,fill='pine',color='white',size=9):
        self.rect(x,y,w,h,fill,True)
        self.text(x+.06,y+.01,w-.12,h-.02,t,size,color,True,'c')
    def footer(self,n,source='LINZ WINTERMARKT  •  DESIGN DOCUMENT  •  18 OCT 2026'):
        self.line(.55,7.12,12.78,7.12,'muted',.45)
        self.text(.58,7.17,10,.19,source,8,'slate')
    def heading(self,kicker,title,subtitle=None):
        self.text(.65,.36,11,.25,kicker.upper(),9,'berry',True)
        self.text(.65,.73,12, .6,title,27,'pine',True)
        if subtitle: self.text(.67,1.37,11.8,.42,subtitle,12,'slate')

# cover
s=base_slide('pine')
s.rect(0,0,W,H,'pine')
# snow hills, lights, market skyline
s.oval(-1.2,4.7,9,4,'pine2'); s.oval(5.7,4.25,9,4.4,'pine2')
s.text(.7,1.2,7,.71,'LINZ',34,'gold',True)
s.text(.7,1.78,7,.71,'WINTERMARKT',34,'cream',True)
s.text(.75,3.0,6.4,.82,'Build the market.\nKeep the queues moving. Welcome Linz.',19,'cream',False)
s.label(.76,4.18,2.15,.38,'VOLKSGARTEN · LINZ','berry','white',10)
s.text(.78,4.75,5.7,.67,'A small, original market-management game\nset at the Goethekreuzung entrance.',12,'ice')
s.text(.78,5.62,3,.22,'TEAM',9,'gold',True)
for i,(name,sid) in enumerate([('JOASH JOHNSON SAMUEL','K12340310'),('ALIBEK ABYLKASYMOV','K12349217'),('BOBBY ALINKAL','K12352652')]):
    s.text(.78,5.9+i*.27,2.6,.24,name,9.5,'cream',True)
    s.text(3.35,5.9+i*.27,1.4,.24,sid,9.5,'ice')
# illustrated night market on right
# wheel
s.oval(9.35,1.05,2.18,2.18,None,'gold')
# manually ellipse no fill via custom shape
# overwrite last shape's fill to none by direct modification
# spokes
for a in range(8):
    import math
    cx,cy,r=10.44,2.14,1.06; q=a*math.pi/4
    s.line(cx-r*math.cos(q),cy-r*math.sin(q),cx+r*math.cos(q),cy+r*math.sin(q),'gold',.8)
s.oval(10.34,2.04,.2,.2,'gold')
# stall cluster
for i,(x,y,col) in enumerate([(7.2,4.7,'berry'),(9.0,4.35,'gold'),(10.8,4.8,'blue')]):
    s.rect(x,y,1.72,1.25,'cream',True)
    s.rect(x-.05,y-.28,1.82,.38,col,True)
    s.rect(x+.16,y+.18,1.4,.17,'gold')
    s.rect(x+.18,y+.48,1.35,.64,'pine2',True)
    for k in range(3): s.oval(x+.35+k*.43,y-.72,.14,.14,'gold')
# tram in distance
s.rect(7.3,3.58,4.8,.48,'ice',True); s.rect(7.55,3.66,4.3,.19,'blue',True)
for k in range(6): s.rect(7.55+k*.67,3.66,.48,.19,'cream',True)
s.text(7.5,3.23,4.3,.24,'TRAM + BUS: AMBIENT LINZ CONTEXT',8,'cream',True,'c')
s.oval(8.9,1.25,.12,.12,'cream'); s.oval(8.2,.75,.08,.08,'cream'); s.oval(11.7,1.0,.11,.11,'cream'); s.oval(12.0,2.7,.09,.09,'cream')
slides.append(s)

# 2 pitch
s=base_slide(); s.heading('01 / THE IDEA','A lively Christmas market, built one smart choice at a time.','Design hypothesis: players enjoy balancing the crowd they attract with the visitors their market can actually serve.')
s.rect(.7,2.05,7.1,2.25,'pine',True)
s.text(1.0,2.32,6.45,.35,'THE PITCH',10,'gold',True)
s.text(1.0,2.82,6.35,1.06,'Build a welcoming market in Linz’s Volksgarten. Place stalls and rides, bring in visitors, earn from service, and improve the market before the festival ends.',19,'cream',True)
s.rect(8.1,2.05,4.52,2.25,'ice',True)
s.text(8.4,2.36,3.9,.32,'PLAYER PROMISE',10,'berry',True)
s.text(8.4,2.8,3.82,.95,'A cozy city square comes alive because of the choices you make.',20,'pine',True)
# three cards
cards=[('PLACE','Choose what to build and where.','01'),('WELCOME','Attract people; watch where they go.','02'),('GROW','Reinvest in capacity or new attractions.','03')]
for i,(a,b,n) in enumerate(cards):
    x=.7+i*4.03
    s.rect(x,4.68,3.72,1.36,'cream',True,line='ice')
    s.oval(x+.18,4.88,.42,.42,'gold')
    s.text(x+.24,4.91,.3,.25,n,9,'pine',True,'c')
    s.text(x+.75,4.82,2.6,.3,a,11,'berry',True)
    s.text(x+.75,5.2,2.72,.58,b,13,'ink',True)
s.text(.75,6.33,11.8,.5,'Audience: cozy building/management fans. Reference boundary: readable placement only; original rules, economy and art. No combat or raids.',9,'berry',True)
s.footer(2,'DESIGN WORKSHOP: IDEA ESSENCE + CORE MECHANICS')
slides.append(s)

# 3 core loop
s=base_slide(); s.heading('01 / OBJECTIVE','Welcome as many visitors as possible before the market closes.','Build stalls and rides, then improve service so more guests can enjoy the market.')
items=[('01','PLAN','Read the space\nand your budget.','ice'),('02','PLACE','Build a stall\nor a ride.','cream'),('03','ATTRACT','Visitors arrive\nand choose a queue.','gold'),('04','SERVE','Capacity and patience\nshape reputation.','redlight'),('05','REINVEST','Earn coins; improve\nthe market.','cream')]
for i,(n,h,b,fill) in enumerate(items):
    x=.63+i*2.56
    s.rect(x,2.42,2.15,1.65,fill,True)
    s.oval(x+.18,2.62,.43,.43,'pine')
    s.text(x+.24,2.66,.3,.22,n,8,'white',True,'c')
    s.text(x+.18,3.15,1.78,.27,h,12,'berry',True,'c')
    s.text(x+.15,3.5,1.86,.43,b,11,'ink',False,'c')
    if i<4: s.text(x+2.15,3.08,.39,.38,'›',26,'pine',True,'c')
s.line(1.65,4.53,11.75,4.53,'muted',1.3,True)
s.rect(2.0,4.93,9.3,1.18,'pine',True)
s.text(2.35,5.16,8.6,.37,'Primary score: unique visitors welcomed before closing',17,'cream',True,'c')
s.text(2.4,5.59,8.5,.27,'Secondary feedback: served · queue abandonments · coins earned · reputation',11,'ice',False,'c')
s.footer(3,'LINZ WINTERMARKT  •  CORE OBJECTIVE')
slides.append(s)

# 4 meaningful decisions
s=base_slide(); s.heading('03 / MEANINGFUL DECISIONS','Attract more people—or serve the crowd you already have?','Both options need a visible benefit, a cost, and a consequence players can understand.')
s.rect(.75,2.02,5.65,2.65,'berry',True)
s.label(1.03,2.29,1.34,.33,'OPTION A','cream','berry',9)
s.text(1.05,2.85,4.9,.45,'Add an attraction',22,'cream',True)
s.text(1.05,3.47,4.82,.9,'A ride raises market attraction and can increase arrivals. It costs more coins and uses a larger plot.',14,'cream')
s.text(1.05,4.24,4.9,.25,'UPSIDE  ·  more potential visitors',10,'gold',True)
s.rect(6.83,2.02,5.65,2.65,'pine',True)
s.label(7.11,2.29,1.34,.33,'OPTION B','gold','pine',9)
s.text(7.13,2.85,4.9,.45,'Upgrade a service',22,'cream',True)
s.text(7.13,3.47,4.82,.9,'A stall upgrade raises capacity and service speed. It costs coins and does less to increase new arrivals.',14,'cream')
s.text(7.13,4.24,4.9,.25,'UPSIDE  ·  shorter waits, happier visitors',10,'gold',True)
s.rect(.75,5.05,11.73,1.05,'ice',True)
s.text(1.05,5.22,2.1,.3,'DESIGN TEST',10,'berry',True)
s.text(3.0,5.19,8.9,.54,'Can players predict each choice, see its effect immediately, and still face a real trade-off?',15,'pine',True)
s.text(.78,6.42,11.6,.3,'First simulated runs: queues rarely overflow at current values, so attraction wins. Tuning capacity pressure is the next design task.',10,'slate')
s.footer(4,'WORKSHOP: INTERESTING DECISIONS ARE CLEAR, INTEGRATED & DOUBLE-EDGED')
slides.append(s)

def shade(col,f):
    col=C.get(col,col); r,g,b=(int(col[i:i+2],16) for i in (0,2,4))
    return '%02X%02X%02X'%tuple(max(0,min(255,round(v*f))) for v in (r,g,b))

def iso_market(s,bx,by,bw,bh):
    """Concept mockup of the angled game camera, drawn as native isometric facets."""
    import math
    CX=math.cos(math.radians(30))
    def P(u,v,z=0): return ((u-v)*CX,(u+v)*.5-z)
    ops=[]  # (depth, kind, data); a stable sort keeps stacked parts in insertion order
    def face(depth,pts,fill): ops.append((depth,'poly',(pts,fill)))
    def ground(depth,u0,u1,v0,v1,fill,z=0): face(depth,[P(u0,v0,z),P(u1,v0,z),P(u1,v1,z),P(u0,v1,z)],fill)
    def box(u,v,w,d,h,col,z=0,top=None,depth=None):
        dp=u+w/2+v+d/2 if depth is None else depth
        face(dp,[P(u,v+d,z),P(u+w,v+d,z),P(u+w,v+d,z+h),P(u,v+d,z+h)],shade(col,.84))
        face(dp,[P(u+w,v,z),P(u+w,v+d,z),P(u+w,v+d,z+h),P(u+w,v,z+h)],shade(col,.68))
        face(dp,[P(u,v,z+h),P(u+w,v,z+h),P(u+w,v+d,z+h),P(u,v+d,z+h)],top or col)
        return dp
    def uface(dp,u,v0,v1,z0,z1,col): face(dp,[P(u,v0,z0),P(u,v1,z0),P(u,v1,z1),P(u,v0,z1)],col)
    def vface(dp,v,u0,u1,z0,z1,col): face(dp,[P(u0,v,z0),P(u1,v,z0),P(u1,v,z1),P(u0,v,z1)],col)
    def oval(dp,cx,cy,rx,ry,fill,line=None,width=1): ops.append((dp,'oval',(cx,cy,rx,ry,fill,line,width)))
    def seg(dp,a,b,col,width): ops.append((dp,'line',(a,b,col,width)))
    def tree(u,v,sc=1,dp=None):
        dp=u+v if dp is None else dp; x,y=P(u,v)
        face(dp,[(x-.07*sc,y),(x+.07*sc,y),(x+.07*sc,y-.35*sc),(x-.07*sc,y-.35*sc)],'5A3F2A')
        for i in range(3):
            y0=y-(.3+i*.42)*sc; y1=y0-.85*sc; w=(.55-.13*i)*sc
            face(dp,[(x-w,y0),(x+w,y0),(x,y1)],'1F4A3C' if i%2 else '2A5C4A')
            face(dp,[(x-w*.32,y1+.27*sc),(x+w*.32,y1+.27*sc),(x,y1)],'F4F7F5')
    def person(u,v,col):
        x,y=P(u,v); dp=u+v+.05
        oval(dp,x,y-.16,.1,.15,col); oval(dp,x,y-.36,.07,.07,'F2D2B0')

    snow='EEF3F2'; road='3B4846'; path='DCCFB6'
    U0,U1,V0,V1=-3.2,14.6,-4.4,11.2
    # Diorama base: snowy ground, earth edges, park lawn, paths and streets.
    face(-100,[P(U0,V0),P(U1,V0),P(U1,V1),P(U0,V1)],snow)
    face(-100,[P(U0,V1),P(U1,V1),P(U1,V1,-.8),P(U0,V1,-.8)],'8C7A63')
    face(-100,[P(U1,V0),P(U1,V1),P(U1,V1,-.8),P(U1,V0,-.8)],'6E604E')
    ground(-99,0,11,0,9.4,'D3E2DC')
    ground(-98,0,11.6,4,5,path); ground(-98,5,6,0,9.4,path)
    face(-98,[P(5.5+1.7*math.cos(t*math.pi/12),4.5+1.7*math.sin(t*math.pi/12)) for t in range(24)],path)
    ground(-97,11.6,13.8,V0,V1,road); ground(-97,U0,U1,-2.2,-.5,road)
    for r in (12.3,12.42,12.95,13.07): ground(-96,r,r+.05,V0,V1,'9AA6A3')
    for i in range(5): ground(-96,11.7+i*.42,11.92+i*.42,4.08,4.92,'F2F2EC')
    for u in range(-3,15,2): ground(-96,u,u+.9,-1.38,-1.3,'F2F2EC')
    ground(-96,11.65,12.05,5.6,8.6,'C9CCC4')

    # City edge: Musiktheater mass and lit facades behind the bus street.
    dp=box(-3.0,.6,2.0,9.0,2.0,'D4CFC0',top='EEF0EC')
    for k in range(16): uface(dp,-1.0,.9+k*.55,1.07+k*.55,.25,1.8,'F3EBD3')
    for u,w,h,col in [(-2.9,3.0,3.0,'B9B2A3'),(.5,2.7,3.9,'C4B9A6'),(3.5,3.2,3.1,'AFA899'),(7.0,2.9,4.3,'BDB3A2'),(10.2,3.8,3.4,'B4AD9F')]:
        dp=box(u,-4.2,w,1.8,h,col,top='F2F4F2')
        z=.5
        while z<h-.4:
            for c in range(int(w/.62)): vface(dp,-2.4,u+.2+c*.62,u+.5+c*.62,z,z+.38,'F7C873')
            z+=.85

    # Ferris wheel (back left).
    dp=box(1.5,1.4,2.0,1.0,.2,'8D7C66'); hx,hy=P(2.5,1.9,3.15)
    for a,b in [((1.7,1.6),(2.5,1.9)),((3.3,1.6),(2.5,1.9)),((1.7,2.2),(2.5,1.9)),((3.3,2.2),(2.5,1.9))]:
        seg(dp,P(*a,.2),(hx,hy),'D9B46A',2.2)
    for k in range(12):
        t=k*math.pi/6; seg(dp,(hx,hy),(hx+2.2*math.cos(t),hy+2.2*math.sin(t)),'E6C783',.6)
    oval(dp,hx,hy,2.2,2.2,None,'E6B75A',2.5); oval(dp,hx,hy,1.95,1.95,None,'E6B75A',.8)
    for k in range(12):
        t=k*math.pi/6+.26; oval(dp,hx+2.2*math.cos(t),hy+2.2*math.sin(t)+.18,.17,.15,['A63D42','4F8992','CE7548','E6B75A'][k%4],'FFF2C8',.5)
    oval(dp,hx,hy,.16,.16,'FFF2C8')

    # Carousel (back right).
    cu,cv,r=9.0,2.3,1.15; dp=cu+cv; rx,ry=1.2247*r,.707*r
    x0,y0=P(cu,cv,0); x1,y1=P(cu,cv,.22); x2,y2=P(cu,cv,1.35); xa,ya=P(cu,cv,2.35)
    oval(dp,x0,y0,rx,ry,'9C8260'); face(dp,[(x0-rx,y0),(x0+rx,y0),(x1+rx,y1),(x1-rx,y1)],'9C8260'); oval(dp,x1,y1,rx,ry,'E9DCC0')
    for k in range(10):
        t=k*math.pi/5; seg(dp,(x1+.85*rx*math.cos(t),y1+.85*ry*math.sin(t)),(x2+.85*rx*math.cos(t),y2+.85*ry*math.sin(t)),'E6B75A',1.2)
        oval(dp,x1+.85*rx*math.cos(t),y1+.85*ry*math.sin(t)-.42,.13,.09,['FFFDF7','CE7548','4F8992'][k%3])
    oval(dp,x2,y2,rx*1.05,ry*1.05,'E6B75A')
    rim=[(x2+rx*1.05*math.cos(math.pi-t*math.pi/10),y2+ry*1.05*math.sin(math.pi-t*math.pi/10)) for t in range(11)]
    face(dp,[(x2-rx*1.05,y2),(xa,ya),(x2+rx*1.05,y2)],'A63D42')
    for k in range(10): face(dp,[(xa,ya),rim[k],rim[k+1]],'A63D42' if k%2 else 'FFFDF7')
    oval(dp,xa,ya-.05,.12,.12,'E6B75A')

    # Wooden stalls with snowy roofs; queues face the camera.
    for u,v,col in [(.6,6.4,'A63D42'),(2.7,6.4,'E6B75A'),(6.5,6.4,'4F8992'),(8.6,6.4,'CE7548'),(6.4,.6,'A63D42'),(.5,.4,'4F8992')]:
        dp=box(u,v,1.5,1.1,.75,'8A5A36',top='6E4529')
        vface(dp,v+1.1,u+.25,u+1.25,.25,.6,'F7C873')
        box(u-.1,v-.1,1.7,1.35,.16,col,z=.75,depth=dp)
        box(u-.1,v-.1,1.7,1.35,.07,'F7F8F6',z=.91,depth=dp)
        for q in range(3): person(u+.45+q*.32,v+1.45+q*.35,['A63D42','4F8992','E6B75A','CE7548','173C36'][(q+int(u))%5])

    # Central Christmas tree, park trees and the crowd.
    tree(5.5,4.5,2.1); ops.append((10.01,'star',P(5.5,4.5,4.25)))
    for k in range(14):
        t=k*2.4; x,y=P(5.5,4.5); oval(10.02,x+math.sin(t)*(.3+k*.06),y-3.3+k*.2,.06,.06,['F7C873','E2564C'][k%2])
    for u,v in [(.4,9.0),(4.1,8.9),(10.5,8.9),(10.5,3.3),(.3,3.4),(4.2,.3),(-2.3,10.4),(2.0,10.6),(6.6,10.5),(9.8,10.4),(14.2,9.6),(14.2,2.2),(14.2,-3.4),(-2.6,-.2)]: tree(u,v,.9)
    crowd=[(1.2,4.3),(2.4,4.7),(3.5,4.2),(4.1,4.8),(7.2,4.3),(8.1,4.7),(9.3,4.4),(10.4,4.6),(11.2,4.3),(5.4,1.5),(5.7,2.7),(5.3,7.0),(5.6,8.2),(6.9,3.4),(4.3,5.8)]
    for i,(u,v) in enumerate(crowd): person(u,v,['A63D42','4F8992','E6B75A','CE7548','FFFDF7','28564D'][i%6])

    # String lights over the main walk.
    for vv in (3.85,5.15):
        pts=[(u/2,vv,1.45-.28*math.sin(math.pi*((u/2)%2.2)/2.2)) for u in range(1,23)]
        for a,b in zip(pts,pts[1:]): seg(a[0]+a[1]+.2,P(*a),P(*b),'F1D9A0',.5)
        for a in pts: x,y=P(*a); oval(a[0]+a[1]+.21,x,y,.06,.06,'FFD36B')

    # Tram on the near street and a bus behind the park.
    for k in range(3):
        v=5.7+k*1.12; dp=box(12.25,v,.75,1.05,.85,'F4F4EE',top='E3E6E2')
        uface(dp,13.0,v+.08,v+.97,.42,.7,'2A4A55'); uface(dp,13.0,v,v+1.05,.1,.3,'C2262E')
    vface(dp,v+1.05,12.3,12.95,.42,.7,'2A4A55')
    dp=box(2.2,-1.95,3.2,.72,.85,'F4F4EE',top='E3E6E2')
    vface(dp,-1.23,2.35,5.25,.42,.7,'2A4A55'); vface(dp,-1.23,2.2,5.4,.1,.3,'C2262E')

    # Fit the drawing into the board, then emit back to front.
    xs=[];ys=[]
    for _,kind,d in ops:
        if kind=='poly': xs+=[q[0] for q in d[0]]; ys+=[q[1] for q in d[0]]
        elif kind=='oval': xs+=[d[0]-d[2],d[0]+d[2]]; ys+=[d[1]-d[3],d[1]+d[3]]
        elif kind=='line': xs+=[d[0][0],d[1][0]]; ys+=[d[0][1],d[1][1]]
    k=min(bw/(max(xs)-min(xs)),bh/(max(ys)-min(ys)))
    ox=bx+(bw-(max(xs)-min(xs))*k)/2-min(xs)*k; oy=by+(bh-(max(ys)-min(ys))*k)/2-min(ys)*k
    T=lambda q:(ox+q[0]*k,oy+q[1]*k)
    a_ns='http://schemas.openxmlformats.org/drawingml/2006/main'; p_ns='http://schemas.openxmlformats.org/presentationml/2006/main'
    for _,kind,d in sorted(ops,key=lambda o:o[0]):
        if kind=='poly': s.poly([T(q) for q in d[0]],d[1])
        elif kind=='oval':
            cx,cy,rx,ry,fill,line,width=d; x,y=T((cx-rx,cy-ry))
            sp=s.oval(x,y,2*rx*k,2*ry*k,fill,line)
            if line: sp.find(f'{{{p_ns}}}spPr').find(f'{{{a_ns}}}ln').set('w',str(round(width*12700)))
        elif kind=='line': s.line(*T(d[0]),*T(d[1]),d[2],d[3])
        elif kind=='star': x,y=T(d); s._shape('star',x-.13,y-.13,.26,.26,'star5','FFD36B')
    # Snowfall and labels sit on top of the scene.
    seed=7
    for i in range(46):
        seed=(seed*1103515245+12345)%2**31; fx=seed/2**31
        seed=(seed*1103515245+12345)%2**31; fy=seed/2**31
        r=.025+.02*(i%3); s.oval(bx+fx*bw,by+fy*bh,r,r,'FFFFFF')
    x,y=T(P(12.7,-1.3)); s.label(x-.05,y-.62,1.38,.24,'GOETHEKREUZUNG','berry','white',7)
    x,y=T(P(1.5,9.6)); s.label(x-.55,y+.05,1.1,.24,'VOLKSGARTEN','gold','pine',7)
    x,y=T(P(-2.5,2.0,2.0)); s.label(x-.55,y-.42,1.1,.22,'MUSIKTHEATER','cream','slate',6.5)
    s.label(bx+.08,by+.06,2.65,.27,'CONCEPT MOCKUP · NOT A GAME SCREENSHOT','cream','berry',7)

# 5 map
s=base_slide(); s.heading('02 / CHOSEN LOCATION','Goethekreuzung, Linz—with the market in the Volksgarten.','Linz trams and buses frame the entrance; Mariendom and Hauptplatz appear on a city overview.')
# angled game-view mockup
s.rect(.68,1.97,7.35,4.67,'pine',True)
iso_market(s,.78,2.3,7.15,4.22)
# layout explainer on right
s.rect(8.38,2.0,4.22,1.15,'cream',True)
s.text(8.65,2.23,3.6,.23,'FIXED SITE FRAME',10,'berry',True)
s.text(8.65,2.55,3.55,.42,'Entrance, paths and trees are not buildable.',11,'ink')
s.rect(8.38,3.35,4.22,1.15,'cream',True)
s.text(8.65,3.58,3.6,.23,'AMBIENT TRANSIT',10,'berry',True)
s.text(8.65,3.9,3.55,.42,'Linz tram + bus move in the background.',11,'ink')
s.rect(8.38,4.7,4.22,1.42,'pine',True)
s.text(8.65,4.93,3.6,.23,'GEOGRAPHY STATUS',10,'gold',True)
s.text(8.65,5.25,3.55,.66,'Current grid and routes are a blockout—not measured or visually verified.',12,'cream',True)
s.text(8.47,6.36,4.0,.19,'Volksgarten → Mariendom → Hauptplatz (city overview)',9,'slate',False,'c')
s.footer(5,'CHOSEN LOCATION  •  GOETHEKREUZUNG, LINZ')
slides.append(s)

# 6 formal elements
s=base_slide(); s.heading('05 / GAME STRUCTURE','The game’s formal elements make the objective measurable.','Mapped from the workshop framework; current rules below are proposed design, not final balance.')
formal=[('PLAYERS','One player','Market manager'),('OBJECTIVE','Maximize','unique visitors welcomed'),('PROCEDURES','Place · inspect · upgrade','Mouse-first controls; pause'),('RULES','Grid + route safety','Queue cells stay open; pay only on valid build'),('RESOURCES','Coins · space · time','Earn coins after service'),('CONFLICT','Attraction vs capacity','Crowd growth can overwhelm queues'),('BOUNDARY','One fixed park site','Paths, trees and roads stay reserved'),('OUTCOME','Festival result','Visitors, served, abandoned, income')]
for i,(a,b,d) in enumerate(formal):
    col=i%4; row=i//4; x=.7+col*3.13; y=2.02+row*2.1
    s.rect(x,y,2.82,1.72,'cream' if row==0 else 'ice',True)
    s.text(x+.18,y+.16,2.42,.24,a,9,'berry',True)
    s.text(x+.18,y+.54,2.42,.37,b,15,'pine',True)
    s.text(x+.18,y+1.04,2.42,.48,d,10,'slate')
s.text(.75,6.43,11.7,.3,'Key acceptance question: does a player understand why visitor count, queue pressure and reputation changed?',11,'pine',True,'c')
s.footer(6,'DESIGN WORKSHOP: FORMAL ELEMENTS')
slides.append(s)

# 7 MDA
s=base_slide(); s.heading('06 / MDA + SYSTEM DYNAMICS','Simple rules should create a lively market—not a spreadsheet.','The intended experience is a design hypothesis. Playtests must confirm whether the feedback is legible and enjoyable.')
cols=[(.72,'MECHANICS','What the game calculates','Place buildings • spend coins\nVisitors choose queues\nService earns money\nReputation affects arrivals','ice'),(4.85,'DYNAMICS','What emerges over time','More attraction → arrivals rise\nMore arrivals → queues lengthen\nBetter service → reputation rises\nIncome → new investment','cream'),(8.98,'AESTHETICS','How it should feel','A cozy, bustling square\nPride in a growing market\nPressure without frustration\nA small Linz winter story','gold')]
for x,h,sub,body,fill in cols:
    s.rect(x,2.2,3.63,2.42,fill,True)
    s.text(x+.24,2.43,3.1,.25,h,11,'berry',True)
    s.text(x+.24,2.83,3.1,.3,sub,13,'pine',True)
    s.text(x+.24,3.35,3.08,1.06,body,12,'ink')
for x in [4.42,8.55]: s.text(x,3.05,.38,.42,'›',28,'pine',True,'c')
# feedback loop
s.rect(1.2,5.08,10.92,.99,'pine',True)
for i,(t,x) in enumerate([('ATTRACTION',1.48),('ARRIVALS',3.67),('QUEUES',5.88),('SERVICE',8.04),('REPUTATION',10.1)]):
    s.label(x,5.35,1.55,.34,t,'gold','pine',8)
    if i<4: s.text(x+1.64,5.32,.4,.33,'→',18,'cream',True,'c')
s.text(1.2,6.3,10.9,.32,'Positive loop: attraction earns attention; reliable service protects reputation and keeps the market growing.',10,'slate',False,'c')
s.footer(7,'DESIGN WORKSHOP: MDA + DYNAMIC ELEMENTS; PLAYTEST IS NEEDED TO UNDERSTAND THEM')
slides.append(s)

# 8 interface
s=base_slide(); s.heading('07 / PLAY EXPERIENCE','Every choice gets immediate, readable feedback.','Prototype control: click to place and inspect · WASD/arrow keys pan · scroll zooms · Escape pauses.')
# frame
s.rect(.72,2.0,8.18,4.6,'pine',True)
s.rect(.95,2.23,7.72,.57,'cream',True)
s.text(1.13,2.36,2.0,.24,'WINTERMARKT',11,'pine',True)
s.text(3.25,2.35,3.95,.25,'350 COINS     12 GUESTS     60% HAPPY',10,'ink',True,'c')
s.label(7.3,2.34,1.11,.32,'09:42','berry','white',9)
# mock map area
s.rect(.95,2.95,7.72,3.39,'tree',True)
s.rect(1.18,4.35,7.28,.35,'path',True); s.rect(4.56,3.12,.38,3.03,'path',True)
for i,(x,y,c) in enumerate([(2.0,3.55,'berry'),(3.0,5.0,'gold'),(5.48,3.65,'blue'),(6.5,5.04,'orange')]):
    s.rect(x,y,.7,.48,'cream',True); s.rect(x-.02,y-.11,.74,.14,c,True)
for x,y in [(1.5,3.25),(3.7,5.55),(7.8,3.2),(7.3,5.45)]: s.oval(x,y,.3,.3,'pine2')
# queue people dots
for x,y in [(5.05,4.2),(5.08,4.5),(5.1,4.79),(5.15,5.08)]: s.oval(x,y,.14,.14,'gold')
s.text(1.1,6.03,2.8,.2,'SCHEMATIC UI MOCK-UP',8,'cream',True)
# inspector
s.rect(9.18,2.0,3.42,4.6,'ice',True)
s.text(9.48,2.3,2.8,.32,'ATTRACTION INFO',10,'berry',True)
s.text(9.48,2.8,2.85,.39,'Punsch Hütte · Lv 1',15,'pine',True)
s.line(9.47,3.32,12.3,3.32,'muted',.7)
s.text(9.48,3.54,2.8,.32,'4 waiting / 1 serving',12,'ink',True)
s.text(9.48,4.02,2.85,.84,'9 coins per visit\n7s service\nCapacity 1 at once\nAttraction +5',11,'slate')
s.label(9.5,5.24,2.76,.48,'UPGRADE · 75 COINS','pine','white',10)
s.text(9.5,5.9,2.78,.38,'Queue pressure and visitor mood stay visible.',10,'pine',True)
s.footer(8,'WORKSHOP: CONTROLS, FEEDBACK, DIGITAL PROTOTYPE  •  KICKOFF RUBRIC P. 10; WORKSHOP P. 34')
slides.append(s)

# 9 scope
s=base_slide(); s.heading('08 / FIRST PLAYABLE','A narrow vertical slice first; polish follows evidence.','A focused prototype will test the market loop before the team expands its content.')
s.rect(.72,2.0,11.9,3.82,'pine',True)
s.text(1.02,2.28,11.3,.29,'MVP — PROVE THE MARKET LOOP',11,'gold',True)
mvp=['One fixed Volksgarten blockout','Three stalls + two simple rides','One entrance; one walking/queue system','Coins from completed service','Attraction, capacity, patience & reputation','10-minute festival + results screen','Pause, restart and clear controls']
for i,t in enumerate(mvp):
    x=1.04+(i//4)*5.9; y=2.9+(i%4)*.62
    s.oval(x,y+.05,.2,.2,'gold'); s.text(x+.36,y,5.2,.3,t,14,'cream')
s.text(.87,6.08,11.55,.7,'Keep scope small. Current status: market source + 8 core tests pass; Unity scene/Play Mode and real site geometry remain unverified.',11,'slate',True,'c')
s.footer(9,'WORKSHOP: SCOPE + PROTOTYPE')
slides.append(s)

# 10 visual direction
s=base_slide(); s.heading('09 / VISUAL + LOCAL IDENTITY','Warm stalls inside a real urban winter setting.','Visual direction: original low-poly 3D, readable from an angled overhead camera; location research must guide the final map.')
# swatches
palette=[('PINE','pine'),('SNOW','cream'),('BERRY','berry'),('LAMPLIGHT','gold'),('ICE','ice')]
for i,(name,col) in enumerate(palette):
    x=.82+i*1.26
    s.rect(x,2.08,.92,.57,col,True,line='muted' if col=='cream' else None)
    s.text(x-.04,2.76,1.02,.2,name,8,'slate',True,'c')
# scene art blocks
s.rect(.78,3.2,6.05,2.73,'pine',True)
s.oval(4.9,3.55,1.18,1.18,None,'gold')
for j in range(6):
    import math
    q=j*math.pi/3; s.line(5.49,4.14,5.49+.57*math.cos(q),4.14+.57*math.sin(q),'gold',.65)
s.rect(1.2,5.18,5.2,.21,'path',True)
for x,y,col in [(1.55,4.3,'berry'),(2.82,4.55,'gold'),(4.1,4.23,'blue')]:
    s.rect(x,y,1.03,.83,'cream',True); s.rect(x-.05,y-.18,1.13,.24,col,True); s.rect(x+.15,y+.22,.74,.37,'pine2',True)
# visual notes right
notes=[('Local, not generic','Volksgarten paths and Goethekreuzung edge define the camera frame.'),('Transit belongs in the scene','Original tram and bus silhouettes move in the background.'),('Upgrade levels read at a glance','More lights, trim and ornaments; same stall footprint.'),('Use references responsibly','Original shapes; cite any external map, photo, sound or model.')]
for i,(h,b) in enumerate(notes):
    y=2.14+i*1.04
    s.text(7.28,y,5.0,.25,h,12,'berry',True)
    s.text(7.28,y+.32,5.0,.49,b,11,'ink')
s.label(.95,6.16,5.7,.34,'SITE GEOMETRY + ROUTES ARE CURRENTLY PROVISIONAL','berry','white',9)
s.footer(10,'LOCATION BRIEF: USE DATED REFERENCES; RESEARCH FIRST, DETAIL LATER')
slides.append(s)

# 11 iteration/playtesting
s=base_slide(); s.heading('10 / TEST PLAN','Test the loop before adding more content.','The workshop calls for rapid prototypes, observed play, and small changes followed by another test.')
steps=[('HYPOTHESIS','Players can tell whether a ride or a capacity upgrade will help their current market.'),('OBSERVE','Can a new player place a building, understand the preview, and explain its effect?'),('MEASURE','Visitors welcomed · served · queue abandonments · time to first upgrade.'),('CHANGE ONE THING','Adjust one or two costs, arrival rates, queue indicators or tutorial cues.'),('RETEST','Repeat the same task; compare observations and log what changed.')]
for i,(h,b) in enumerate(steps):
    y=1.97+i*.85
    s.oval(.83,y+.1,.42,.42,'pine' if i%2==0 else 'berry')
    s.text(.91,y+.17,.25,.22,str(i+1),9,'white',True,'c')
    s.text(1.5,y,2.2,.28,h,10,'berry',True)
    s.text(3.75,y,8.5,.48,b,12,'ink')
    if i<4: s.line(1.04,y+.54,1.04,y+.81,'muted',1.0)
s.rect(.82,6.31,11.6,.47,'ice',True)
s.text(1.06,6.39,11.12,.26,'Evidence status: no external playtests or balance results have been collected yet. First class playtest milestone: 1 Dec 2026.',10,'pine',True,'c')
s.footer(11,'DESIGN WORKSHOP: ITERATIVE DESIGN + PROTOTYPING')
slides.append(s)

# 12 timeline
s=base_slide(); s.heading('11 / COURSE ROADMAP','A weekly design document; playable evidence by December.','Dates follow the 2026/27 course kickoff schedule.')
# timeline line
s.line(1.12,3.1,12.15,3.1,'pine',2.2)
road=[('19 OCT','Initial design\ndocument due','NOW','berry'),('20 OCT','Progress\npresentation','NEXT','gold'),('17 NOV','Progress +\ndesign update','WEEK 5','blue'),('01 DEC','Class playtest:\nplayable prototype','MILESTONE','pine'),('12 JAN','Class playtest:\nalmost finished','MILESTONE','orange'),('26 JAN','Final\npresentation','PITCH','berry'),('01 FEB','Final submission\n23:59','DELIVERY','pine')]
xs=[1.0,2.82,4.64,6.46,8.28,10.1,11.92]
for i,(date,desc,tag,col) in enumerate(road):
    x=xs[i]
    s.oval(x,2.89,.42,.42,col)
    if i%2==0:
        s.text(x-.35,1.98,1.12,.26,date,10,'berry',True,'c'); s.text(x-.42,2.24,1.3,.52,desc,10,'ink',True,'c')
    else:
        s.text(x-.4,3.56,1.22,.26,date,10,'berry',True,'c'); s.text(x-.52,3.9,1.46,.55,desc,10,'ink',True,'c')
    s.label(x-.35,4.94,.98,.3,tag,col,'white' if col!='gold' else 'pine',7)
s.rect(.86,5.59,11.53,.77,'ice',True)
s.text(1.08,5.77,11.08,.36,'Update each week: decisions · task owners · planned dates · actual completion · test evidence · next changes',11,'pine',True,'c')
s.text(.91,6.49,11.3,.22,'Initial design document due Mon 19 Oct, 23:59 · progress presentation Tue 20 Oct.',9,'slate',False,'c')
s.footer(12,'COURSE KICKOFF SCHEDULE P. 6 + SUBMISSION BRIEF SHARED BY THE TEAM')
slides.append(s)

# 13 team responsibilities
s=base_slide(); s.heading('03 / TEAM RESPONSIBILITIES','The work is divided equally across the team.','Each member owns one area of the game, and all three share the design deck, playtests and presentations.')
# table columns
x0=.7; widths=[3.2,5.15,2.7]; headers=['TEAM MEMBER / ROLE','RESPONSIBILITY','PLANNED']
xx=x0
for w,h in zip(widths,headers):
    s.rect(xx,2.0,w-.04,.47,'pine',True)
    s.text(xx+.1,2.1,w-.22,.2,h,8,'cream',True)
    xx+=w
rows=[('Joash Johnson Samuel\nK12340310 · Design + balance','Game rules, economy and attraction-vs-capacity tuning; coordinates the weekly deck.','Balanced loop\nBy 1 Dec'),('Alibek Abylkasymov\nK12349217 · Gameplay + tech','Visitor flow, placement, shops, rides and upgrades in Unity; builds and testing.','Playable prototype\nBy 1 Dec'),('Bobby Alinkal\nK12352652 · World + UI','Volksgarten location, trams and buses, visual style and player interface.','Location + UI pass\nBy 1 Dec')]
for r,row in enumerate(rows):
    y=2.56+r*1.02; xx=x0
    for j,(w,val) in enumerate(zip(widths,row)):
        fill='cream' if r%2==0 else 'ice'
        s.rect(xx,y,w-.04,.91,fill,True)
        s.text(xx+.1,y+.1,w-.24,.71,val,9.5,'pine' if j==0 else 'ink',j==0)
        xx+=w
s.text(.9,5.93,11.4,.42,'Equal share for everyone: one owned area each + shared deck, playtests (1 Dec, 12 Jan) and presentations.',9,'berry',True,'c')
s.footer(13,'LINZ WINTERMARKT  •  TEAM RESPONSIBILITIES')
slides.append(s)

# closing
s=base_slide('pine')
s.rect(0,0,W,H,'pine')
s.oval(-1.0,5.3,8.0,3.0,'pine2'); s.oval(7.0,5.1,8.0,3.2,'pine2')
s.text(.82,.72,6.0,.28,'LINZ WINTERMARKT',11,'gold',True)
s.text(.82,2.12,9.8,.9,'THANK YOU',38,'cream',True)
s.line(.86,3.3,4.5,3.3,'gold',2)
s.text(.86,3.68,8.4,.6,'Build the market. Welcome Linz.',21,'ice')
s.footer(16,'JOASH JOHNSON SAMUEL  ·  ALIBEK ABYLKASYMOV  ·  BOBBY ALINKAL')
slides.append(s)

# 14 status/open risks
s=base_slide(); s.heading('13 / CURRENT STATUS','A design with prototype source; not yet a verified playable build.','This is the honest starting point for weekly iteration—not a claim that the final game is done.')
status=[('DESIGN','Concept, core loop, MDA, scope and location brief documented.','SOURCE READY'),('SIMULATION','Placement, routing, service, queues, upgrades and visitor scoring implemented in source.','8 CORE TESTS PASS'),('UNITY','API compilation succeeds; editor scene import / Play Mode remain unverified here.','NEEDS LOCAL RUN'),('LOCATION','Blockout is schematic; paths, roads, tracks and vehicle routes are not surveyed.','RESEARCH NEEDED')]
for i,(h,b,t) in enumerate(status):
    x=.78+(i%2)*6.0; y=2.02+(i//2)*1.48
    s.rect(x,y,5.62,1.17,'cream' if i%2==0 else 'ice',True)
    s.text(x+.2,y+.15,2.1,.22,h,9,'berry',True)
    s.text(x+.2,y+.45,3.85,.52,b,10,'ink')
    s.label(x+4.04,y+.2,1.35,.29,t,'pine' if i==1 else 'berry','white',7)
s.rect(.82,5.33,11.6,.86,'pine',True)
s.text(1.08,5.54,11.05,.38,'Next decisions: confirm 3-person roster · launch Unity on a normal workstation · verify one complete visitor/service/upgrade loop.',12,'cream',True,'c')
s.text(.88,6.42,11.5,.31,'Main risks: geographic scope, unknown team allocation, balancing demand/capacity, and losing time to visual polish before the loop is tested.',10,'slate',False,'c')
s.footer(14,'STATUS AS OF 17 OCT 2026  •  SEE REPOSITORY README + TESTING LOG FOR VERIFICATION DETAILS')
slides.append(s)

# 15 sources and living log
s=base_slide(); s.heading('14 / SOURCES + WEEKLY CHANGE LOG','The design document will change as the game is tested.','Version 0.1 records the chosen direction and the first testable design hypotheses.')
s.rect(.74,1.95,5.74,4.48,'cream',True)
s.text(1.02,2.22,5.05,.27,'COURSE MATERIALS',10,'berry',True)
sources=[('GameDev 2026 Kickoff','pp. 6–8, 13–15 · schedule, submission, team work'),('GameDev 2026 Design Workshop','pp. 4, 6–9, 16–21, 27–34 · frameworks, decisions, scope, iteration'),('Moodle assignment brief','Initial deck due 19 Oct 2026, 23:59 · screenshot supplied 17 Oct')]
for i,(h,b) in enumerate(sources):
    y=2.75+i*.86
    s.text(1.04,y,4.95,.24,h,11,'pine',True)
    s.text(1.04,y+.31,4.95,.4,b,9,'slate')
s.rect(6.8,1.95,5.78,4.48,'ice',True)
s.text(7.08,2.22,5.1,.27,'LOCAL REFERENCES + VERSION 0.1 LOG',10,'berry',True)
links=[('Official market arrival','christkindlmarkt-linz.at/allgemeines/anreise'),('City market-area plan','linz.at/serviceguide/brochure.php?id=408'),('City 2025 market change','linz.at/medienservice/2025/202511_133138.php')]
for i,(h,b) in enumerate(links):
    y=2.72+i*.68
    s.text(7.1,y,4.97,.22,h,10,'pine',True)
    s.text(7.1,y+.27,5.1,.22,b,8,'slate')
s.line(7.1,4.9,12.24,4.9,'muted',.7)
s.text(7.1,5.08,5.0,.24,'18 OCT 2026 · V0.1',10,'pine',True)
s.text(7.1,5.42,5.0,.72,'Pivoted to an original Linz market-management game. Defined the attraction-versus-capacity decision, an MVP, a provisional site plan, and a first playtest milestone.',10,'ink')
s.text(.87,6.58,11.7,.26,'Diagrams are original editable shapes. JKU logo supplied by the team; it appears on every slide.',9,'slate',False,'c')
s.footer(15,'UPDATE THIS DECK WEEKLY; KEEP DECISIONS, OWNERS, DATES AND PLAYTEST EVIDENCE')
slides.append(s)

# Submission deck: cover, objective, meaningful decision, location, formal
# elements, MDA, first playable, team and closing.
keep = [1, 3, 4, 5, 6, 7, 9, 13, 14]
slides = [slides[i - 1] for i in keep]
_ROOTS[:] = [_ROOTS[i - 1] for i in keep]
renumber = {
    2: '01 / OBJECTIVE',
    3: '02 / MEANINGFUL DECISIONS',
    4: '03 / CHOSEN LOCATION',
    5: '04 / GAME STRUCTURE',
    6: '05 / MDA + SYSTEM DYNAMICS',
    7: '06 / FIRST PLAYABLE',
    8: '07 / TEAM RESPONSIBILITIES',
    9: '08 / THANK YOU',
}
for index, root in enumerate(_ROOTS, 1):
    text_nodes = root.findall('.//{http://schemas.openxmlformats.org/drawingml/2006/main}t')
    if text_nodes and index in renumber:
        text_nodes[0].text = renumber[index]

# Place the supplied JKU wordmark in the same safe top-right position on every
# slide. The source PNG has generous transparent padding; srcRect trims that
# padding without modifying the user's asset.
if not LOGO.is_file():
    raise FileNotFoundError(f'JKU logo not found: {LOGO}')
for index, slide in enumerate(slides):
    if index == 0:
        slide.rect(11.94,.25,1.22,.59,'paper',True,name='JKU logo background')
    pic=ET.SubElement(slide.tree,'{http://schemas.openxmlformats.org/presentationml/2006/main}pic')
    nv=ET.SubElement(pic,'{http://schemas.openxmlformats.org/presentationml/2006/main}nvPicPr')
    ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}cNvPr',{'id':str(slide.i),'name':'JKU logo'})
    slide.i+=1
    cnv=ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}cNvPicPr')
    ET.SubElement(cnv,'{http://schemas.openxmlformats.org/drawingml/2006/main}picLocks',{'noChangeAspect':'1'})
    ET.SubElement(nv,'{http://schemas.openxmlformats.org/presentationml/2006/main}nvPr')
    blipfill=ET.SubElement(pic,'{http://schemas.openxmlformats.org/presentationml/2006/main}blipFill')
    ET.SubElement(blipfill,'{http://schemas.openxmlformats.org/drawingml/2006/main}blip',{'{http://schemas.openxmlformats.org/officeDocument/2006/relationships}embed':'rId2'})
    ET.SubElement(blipfill,'{http://schemas.openxmlformats.org/drawingml/2006/main}srcRect',{'l':'11000','t':'18500','r':'11000','b':'18500'})
    ET.SubElement(blipfill,'{http://schemas.openxmlformats.org/drawingml/2006/main}stretch').append(ET.Element('{http://schemas.openxmlformats.org/drawingml/2006/main}fillRect'))
    sppr=ET.SubElement(pic,'{http://schemas.openxmlformats.org/presentationml/2006/main}spPr')
    xf=ET.SubElement(sppr,'{http://schemas.openxmlformats.org/drawingml/2006/main}xfrm')
    ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}off',{'x':emu(12.03),'y':emu(.29)})
    ET.SubElement(xf,'{http://schemas.openxmlformats.org/drawingml/2006/main}ext',{'cx':emu(1.04),'cy':emu(.50)})
    ET.SubElement(sppr,'{http://schemas.openxmlformats.org/drawingml/2006/main}prstGeom',{'prst':'rect'}).append(ET.Element('{http://schemas.openxmlformats.org/drawingml/2006/main}avLst'))

# XML package serialization
P='http://schemas.openxmlformats.org/presentationml/2006/main'; A='http://schemas.openxmlformats.org/drawingml/2006/main'; R='http://schemas.openxmlformats.org/officeDocument/2006/relationships'; REL='http://schemas.openxmlformats.org/package/2006/relationships'; CT='http://schemas.openxmlformats.org/package/2006/content-types'
ET.register_namespace('a',A); ET.register_namespace('r',R); ET.register_namespace('p',P)
def xml_bytes(root): return ET.tostring(root,encoding='utf-8',xml_declaration=True)
def rels(items):
    root=ET.Element(f'{{{REL}}}Relationships')
    for i,(typ,target) in enumerate(items,1): ET.SubElement(root,f'{{{REL}}}Relationship',{'Id':f'rId{i}','Type':typ,'Target':target})
    return xml_bytes(root)
files={}
# presentation root and slide refs
pres=ET.Element(f'{{{P}}}presentation',{'saveSubsetFonts':'1','autoCompressPictures':'1'})
master_ids=ET.SubElement(pres,f'{{{P}}}sldMasterIdLst'); ET.SubElement(master_ids,f'{{{P}}}sldMasterId',{'id':'2147483648',f'{{{R}}}id':'rId1'})
# slide size + notes size
sldids=ET.SubElement(pres,f'{{{P}}}sldIdLst')
for i in range(len(slides)): ET.SubElement(sldids,f'{{{P}}}sldId',{'id':str(256+i),f'{{{R}}}id':f'rId{i+2}'})
ET.SubElement(pres,f'{{{P}}}sldSz',{'cx':emu(W),'cy':emu(H),'type':'screen16x9'})
ET.SubElement(pres,f'{{{P}}}notesSz',{'cx':emu(7.5),'cy':emu(10)})
ET.SubElement(pres,f'{{{P}}}defaultTextStyle')
files['ppt/presentation.xml']=xml_bytes(pres)
files['ppt/_rels/presentation.xml.rels']=rels([(f'{R}/slideMaster','slideMasters/slideMaster1.xml')]+[(f'{R}/slide',f'slides/slide{i}.xml') for i in range(1,len(slides)+1)])
# master and layout
master=ET.Element(f'{{{P}}}sldMaster'); cs=ET.SubElement(master,f'{{{P}}}cSld'); tree=ET.SubElement(cs,f'{{{P}}}spTree')
nv=ET.SubElement(tree,f'{{{P}}}nvGrpSpPr'); ET.SubElement(nv,f'{{{P}}}cNvPr',{'id':'1','name':''}); ET.SubElement(nv,f'{{{P}}}cNvGrpSpPr'); ET.SubElement(nv,f'{{{P}}}nvPr')
grp=ET.SubElement(tree,f'{{{P}}}grpSpPr'); xf=ET.SubElement(grp,f'{{{A}}}xfrm'); ET.SubElement(xf,f'{{{A}}}off',{'x':'0','y':'0'}); ET.SubElement(xf,f'{{{A}}}ext',{'cx':'0','cy':'0'}); ET.SubElement(xf,f'{{{A}}}chOff',{'x':'0','y':'0'}); ET.SubElement(xf,f'{{{A}}}chExt',{'cx':'0','cy':'0'})
ET.SubElement(master,f'{{{P}}}clrMap',{'bg1':'lt1','tx1':'dk1','bg2':'lt2','tx2':'dk2','accent1':'accent1','accent2':'accent2','accent3':'accent3','accent4':'accent4','accent5':'accent5','accent6':'accent6','hlink':'hlink','folHlink':'folHlink'})
cl=ET.SubElement(master,f'{{{P}}}sldLayoutIdLst'); ET.SubElement(cl,f'{{{P}}}sldLayoutId',{'id':'1',f'{{{R}}}id':'rId1'})
files['ppt/slideMasters/slideMaster1.xml']=xml_bytes(master)
files['ppt/slideMasters/_rels/slideMaster1.xml.rels']=rels([(f'{R}/slideLayout','../slideLayouts/slideLayout1.xml'),(f'{R}/theme','../theme/theme1.xml')])
layout=ET.Element(f'{{{P}}}sldLayout',{'type':'blank','preserve':'1'}); c=ET.SubElement(layout,f'{{{P}}}cSld'); tr=ET.SubElement(c,f'{{{P}}}spTree'); nv=ET.SubElement(tr,f'{{{P}}}nvGrpSpPr'); ET.SubElement(nv,f'{{{P}}}cNvPr',{'id':'1','name':''}); ET.SubElement(nv,f'{{{P}}}cNvGrpSpPr'); ET.SubElement(nv,f'{{{P}}}nvPr'); grp=ET.SubElement(tr,f'{{{P}}}grpSpPr'); xf=ET.SubElement(grp,f'{{{A}}}xfrm'); ET.SubElement(xf,f'{{{A}}}off',{'x':'0','y':'0'}); ET.SubElement(xf,f'{{{A}}}ext',{'cx':'0','cy':'0'}); ET.SubElement(xf,f'{{{A}}}chOff',{'x':'0','y':'0'}); ET.SubElement(xf,f'{{{A}}}chExt',{'cx':'0','cy':'0'}); ET.SubElement(layout,f'{{{P}}}clrMapOvr').append(ET.Element(f'{{{A}}}masterClrMapping'))
files['ppt/slideLayouts/slideLayout1.xml']=xml_bytes(layout)
files['ppt/slideLayouts/_rels/slideLayout1.xml.rels']=rels([(f'{R}/slideMaster','../slideMasters/slideMaster1.xml')])
# theme
th=ET.Element(f'{{{A}}}theme',{'name':'Linz Wintermarkt'}); te=ET.SubElement(th,f'{{{A}}}themeElements'); scheme=ET.SubElement(te,f'{{{A}}}clrScheme',{'name':'Wintermarkt'})
for tag,col in [('dk1',C['ink']),('lt1',C['white']),('dk2',C['pine']),('lt2',C['paper']),('accent1',C['pine']),('accent2',C['berry']),('accent3',C['gold']),('accent4',C['blue']),('accent5',C['ice']),('accent6',C['orange']),('hlink','0563C1'),('folHlink','954F72')]: ET.SubElement(scheme,f'{{{A}}}{tag}').append(ET.Element(f'{{{A}}}srgbClr',{'val':col}))
fonts=ET.SubElement(te,f'{{{A}}}fontScheme',{'name':'Arial'}); major=ET.SubElement(fonts,f'{{{A}}}majorFont'); ET.SubElement(major,f'{{{A}}}latin',{'typeface':FONT}); ET.SubElement(major,f'{{{A}}}ea',{'typeface':''}); ET.SubElement(major,f'{{{A}}}cs',{'typeface':''}); minor=ET.SubElement(fonts,f'{{{A}}}minorFont'); ET.SubElement(minor,f'{{{A}}}latin',{'typeface':FONT}); ET.SubElement(minor,f'{{{A}}}ea',{'typeface':''}); ET.SubElement(minor,f'{{{A}}}cs',{'typeface':''})
fmt=ET.SubElement(te,f'{{{A}}}fmtScheme',{'name':'Wintermarkt'}); fills=ET.SubElement(fmt,f'{{{A}}}fillStyleLst')
for _ in range(3): ET.SubElement(fills,f'{{{A}}}solidFill').append(ET.Element(f'{{{A}}}schemeClr',{'val':'phClr'}))
lines=ET.SubElement(fmt,f'{{{A}}}lnStyleLst')
for w in (6350,12700,19050):
    ln=ET.SubElement(lines,f'{{{A}}}ln',{'w':str(w)}); ET.SubElement(ln,f'{{{A}}}solidFill').append(ET.Element(f'{{{A}}}schemeClr',{'val':'phClr'}))
effects=ET.SubElement(fmt,f'{{{A}}}effectStyleLst')
for _ in range(3): ET.SubElement(effects,f'{{{A}}}effectStyle').append(ET.Element(f'{{{A}}}effectLst'))
bgfills=ET.SubElement(fmt,f'{{{A}}}bgFillStyleLst')
for _ in range(3): ET.SubElement(bgfills,f'{{{A}}}solidFill').append(ET.Element(f'{{{A}}}schemeClr',{'val':'phClr'}))
files['ppt/theme/theme1.xml']=xml_bytes(th)
for i,s in enumerate(slides,1):
    r=_ROOTS[i-1]
    files[f'ppt/slides/slide{i}.xml']=xml_bytes(r)
    files[f'ppt/slides/_rels/slide{i}.xml.rels']=rels([(f'{R}/slideLayout','../slideLayouts/slideLayout1.xml'),(f'{R}/image','../media/JKU_Logo.png')])
files['ppt/media/JKU_Logo.png']=LOGO.read_bytes()
# metadata
files['docProps/core.xml']='''<?xml version="1.0" encoding="UTF-8" standalone="yes"?><cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><dc:title>Linz Wintermarkt — Initial Design Document</dc:title><dc:creator>Joash Johnson</dc:creator><dc:subject>Game design document v0.1</dc:subject><dc:description>Initial course design-document slide deck; prototype status and geography limitations are stated explicitly.</dc:description><dcterms:created xsi:type="dcterms:W3CDTF">2026-10-18T00:00:00Z</dcterms:created></cp:coreProperties>'''.encode()
files['docProps/app.xml']=f'''<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"><Application>Microsoft PowerPoint</Application><PresentationFormat>On-screen Show (16:9)</PresentationFormat><Slides>{len(slides)}</Slides><Company> </Company></Properties>'''.encode()
# content types
ct=ET.Element(f'{{{CT}}}Types'); ET.SubElement(ct,f'{{{CT}}}Default',{'Extension':'rels','ContentType':'application/vnd.openxmlformats-package.relationships+xml'}); ET.SubElement(ct,f'{{{CT}}}Default',{'Extension':'xml','ContentType':'application/xml'}); ET.SubElement(ct,f'{{{CT}}}Default',{'Extension':'png','ContentType':'image/png'})
for part,ctype in [('ppt/presentation.xml','application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml'),('ppt/slideMasters/slideMaster1.xml','application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml'),('ppt/slideLayouts/slideLayout1.xml','application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml'),('ppt/theme/theme1.xml','application/vnd.openxmlformats-officedocument.theme+xml'),('docProps/core.xml','application/vnd.openxmlformats-package.core-properties+xml'),('docProps/app.xml','application/vnd.openxmlformats-officedocument.extended-properties+xml')]: ET.SubElement(ct,f'{{{CT}}}Override',{'PartName':'/'+part,'ContentType':ctype})
for i in range(1,len(slides)+1): ET.SubElement(ct,f'{{{CT}}}Override',{'PartName':f'/ppt/slides/slide{i}.xml','ContentType':'application/vnd.openxmlformats-officedocument.presentationml.slide+xml'})
files['[Content_Types].xml']=xml_bytes(ct)
files['_rels/.rels']=rels([('http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument','ppt/presentation.xml'),('http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties','docProps/core.xml'),('http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties','docProps/app.xml')])
OUT.parent.mkdir(parents=True,exist_ok=True)
with ZipFile(OUT,'w',ZIP_DEFLATED) as z:
    for name,data in files.items(): z.writestr(name,data)
print(f'Wrote {OUT} ({len(slides)} slides, {OUT.stat().st_size:,} bytes)')
