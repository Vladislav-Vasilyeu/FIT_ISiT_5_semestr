#ifndef LIBPLANETS
#define LIBPLANETS 1
const double pi = 3.14159;


struct CSizeD
{
	double cx;
	double cy;
};
//-------------------------------------------------------------------------------
struct CRectD
{
	double left;
	double top;
	double right;
	double bottom;
	CRectD() { left = top = right = bottom = 0; };
	CRectD(double l, double t, double r, double b);
	void SetRectD(double l, double t, double r, double b);
	CSizeD SizeD();		// возвращает ширину/высоту прямоугольной области
};
//-------------------------------------------------------------------------------

CMatrix CreateTranslate2D(double dx, double dy);
CMatrix CreateRotate2D(double fi);
CMatrix SpaceToWindow(CRectD& rs, CRect& rw);
void SetMyMode(CDC& dc, CRectD& RS, CRect& RW);



class CBlade
{
	CRect MainPoint;
	CRect FirstTop;
	CRect SecondTop;
	CRect FirstBootom;
	CRect SecondBootom;
	CRect WayRotation;
	CMatrix FTCoords;
	CMatrix STCoords;
	CMatrix FBCoords;
	CMatrix SBCoords;
	CMatrix FTHCoords;
	CMatrix STHCoords;
	CMatrix FBHCoords;
	CMatrix SBHCoords;
	CRect RW;		   // область в окне
	CRectD RS;		   // область в мировых координатах
	double wPoint;		// скорость вращения
	double fiSB;
	double fiFB;
	double fiST;
	double fiFT;
	double fiHSB;
	double fiHFB;
	double fiHST;
	double fiHFT;

	double dt;		   // шаг времени, сек.
public:
	CBlade();
	void DrawTriangle(const CMatrix& FTCoords, const CMatrix& STCoords, CDC& dc, bool color, const CMatrix& M);
	void SetDT(double dtx) { dt = dtx; };	// установить шаг времени
	void SetNewCoords();					// вычислить новые координаты
	void GetRS(CRectD& RSX);				// вернуть RS
	CRect GetRW() { return RW; };			// вернуть RW
	void Draw(CDC& dc);						// отрисовка
};


#endif

