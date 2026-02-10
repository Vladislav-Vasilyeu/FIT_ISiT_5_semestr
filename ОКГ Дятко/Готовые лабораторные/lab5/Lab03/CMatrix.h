#pragma once
#include <fstream>
using namespace std;
#ifndef CMATRIXH
# define CMATRIXH 1
class CMatrix
{
	double **array;
	int n_rows;							// число строк
	int n_cols;							// число столбцов
public:
	CMatrix();							// конструктор по умолчанию (1 x 1)	
	CMatrix(int, int);		    		// конструктор	
	CMatrix(int);						// конструктор - вектор (N строк)
	CMatrix(const CMatrix&);			// конструктор копирования
	CMatrix(ifstream &file)
	{
		int r = 0;
		int c = 0;
		file >> r;
		file >> c;

		// Nrow - число строк
		// Ncol - число столбцов

		n_rows = r;
		n_cols = c;
		array = new double*[n_rows];
		for (int i = 0; i < n_rows; i++) array[i] = new double[n_cols];

		for (int i = 0; i < n_rows; i++)
			for (int j = 0; j < n_cols; j++) file >> array[i][j];
	}
	~CMatrix();
	double &operator()(int, int);        // доступ к элементу (не const)
	double &operator()(int);            // доступ к элементу в векторе (не const)

	double operator()(int, int) const;  // доступ к элементу (const)
	double operator()(int) const;       // доступ к элементу в векторе (const)

	CMatrix operator-();			    // унарный "-"
	CMatrix operator=(const CMatrix&);	// присваивание M1=M2

	CMatrix operator*(const CMatrix&) const;        // умножение: M1 * M2
	CMatrix operator+(const CMatrix&) const;	    // сложение
	CMatrix operator-(const CMatrix&) const;	    // вычитание

	CMatrix operator+(double) const;		    // M + a
	CMatrix operator-(double) const;		    // M - a

	friend std::ostream& operator<<(std::ostream& os, const CMatrix& matrix)
	{
		os << matrix.rows() << ' ' << matrix.cols() << '\n';

		for (int i = 0; i < matrix.rows(); i++)
		{
			for (int k = 0; k < matrix.cols(); k++)
			{
				os << matrix(i, k) << " ";
			}
			os << '\n';
		}
		return os;
	}
	int rows()const { return n_rows; };   // возвращает число строк
	int cols()const { return n_cols; };    // возвращает число столбцов

	CMatrix Transp() const;				    // транспонированная матрица
	CMatrix GetRow(int) const;			    // возвращает строку
	CMatrix GetRow(int, int, int) const;
	CMatrix GetCol(int) const;			    // возвращает столбец
	CMatrix GetCol(int, int, int) const;

	CMatrix RedimMatrix(int, int);	    // изменить размер матрицы (с уничтожением данных)
	CMatrix RedimData(int, int);         // изменить размер матрицы (с сохранением возможных данных)

	CMatrix RedimMatrix(int);	        // изменить размер (вектор)
	CMatrix RedimData(int);             // изменить размер (вектор) с сохранением

	double MaxElement() const;		  	// максимальный элемент
	double MinElement() const;				// минимальный элемент
};


#endif