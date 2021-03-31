using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCOI_2_R
{
    /// <summary>
    /// Предоставляет методы для интерполяции сплайнов.
    /// </summary>
    public static class SplineInterpolation
    {
        /// <summary>
        /// Возвращает массив координат <see cref="Point"/> заданного сплайна.
        /// </summary>
        /// <param name="interpolationNodes"> Массив <see cref="Point"/> содержащий узлы интерполяции.</param>
        public static Point[] SplineBezierOptimal(Point[] interpolationNodes)
        {
            return SplineBezier(interpolationNodes, StandartParameter(interpolationNodes));
        }


        // Расчет координат сплайна
        private static Point[] SplineBezier(Point[] interpolationNodes, Matrix controlPoint)
        {
            Point[] point = new Point[0]; //Массив содержит рассчитываемые координаты

            // Итератор кривых
            for (int numberCurve = 0; numberCurve < interpolationNodes.GetLength(0) - 1; numberCurve++)
            {


                // Рассчт координат либо по Х либо по Y
                if (Math.Abs(interpolationNodes[numberCurve + 1].X - interpolationNodes[numberCurve].X) >= Math.Abs(interpolationNodes[numberCurve + 1].Y - interpolationNodes[numberCurve].Y))
                {
                    int startPosicion = point.GetLength(0); // Начальный индекс расширенной части массива 

                    // Увеличивается на число, равное полному количеству точек кривой - 1, т.к. последняя точка не включается
                    Array.Resize<Point>(ref point,(int)(point.GetLength(0) + Math.Abs(interpolationNodes[numberCurve + 1].X - interpolationNodes[numberCurve].X)));

                    // Задаем начальную координату Y
                    point[startPosicion].Y = interpolationNodes[numberCurve].Y;

                    // t - линейный параметр кубической функции Безье, dt  - прирощение.
                    // dt расчитанно таким образом, что позволяет расчитать значения для каждой координаты наибольшей орты
                    double dt = (double)1 / Math.Abs(interpolationNodes[numberCurve + 1].X - interpolationNodes[numberCurve].X);
                    double t = dt; // Не ноль, по тому, что "первая итерация" пропущенна, т.к. координаты начала заданы 

                    // Уравнение кубичиской кривой Безье
                    for (int i = 1; i + startPosicion < point.GetLength(0); i++)
                    {
                        point[startPosicion + i].Y = (int)Math.Round(Math.Pow((1 - t), 3) * interpolationNodes[numberCurve].Y +
                                                                     Math.Pow((1 - t), 2) * (double)(controlPoint[2 * numberCurve, 1]) * 3 * t +
                                                                     Math.Pow(t, 2) * (double)(controlPoint[2 * numberCurve + 1, 1]) * 3 * (1 - t) +
                                                                     Math.Pow(t, 3) * interpolationNodes[numberCurve + 1].Y);
                        t += dt;
                    }
                    // Линейно дополняем Х
                    for (int i = 0; i + startPosicion < point.GetLength(0); i++)
                    {
                        point[startPosicion + i].X = (Math.Sign(interpolationNodes[numberCurve + 1].X - interpolationNodes[numberCurve].X) * i) +
                                                     interpolationNodes[numberCurve].X;
                    }
                }
                else
                {
                    int startPosicion = point.GetLength(0);

                    Array.Resize<Point>(ref point, (int)(point.GetLength(0) + Math.Abs(interpolationNodes[numberCurve + 1].Y - interpolationNodes[numberCurve].Y)));

                    point[startPosicion].X = interpolationNodes[numberCurve].X;

                    double dt = (double)1 / Math.Abs(interpolationNodes[numberCurve + 1].Y - interpolationNodes[numberCurve].Y);
                    double t = dt;

                    for (int i = 1; i + startPosicion < point.GetLength(0); i++)
                    {
                        point[startPosicion + i].X = (int)Math.Round(Math.Pow((1 - t), 3) * interpolationNodes[numberCurve].X +
                                                                      Math.Pow((1 - t), 2) * (double)(controlPoint[numberCurve, 0]) * 3 * t +
                                                                      Math.Pow(t, 2) * (double)(controlPoint[numberCurve + 1, 0]) * 3 * (1 - t) +
                                                                      Math.Pow(t, 3) * interpolationNodes[numberCurve + 1].X);
                        t += dt;
                    }

                    for (int i = 0; i + startPosicion < point.GetLength(0); i++)
                    {
                        point[startPosicion + i].Y = (Math.Sign(interpolationNodes[numberCurve + 1].Y - interpolationNodes[numberCurve].Y) * i) +
                                                     interpolationNodes[numberCurve].Y;
                    }
                }
            }

            // Добовляем элемент вмассив и заносим в него координаты последней точки сплайна
            Array.Resize<Point>(ref point, point.GetLength(0) + 1);

            point[point.GetLength(0) - 1].X = interpolationNodes[interpolationNodes.GetLength(0) - 1].X;
            point[point.GetLength(0) - 1].Y = interpolationNodes[interpolationNodes.GetLength(0) - 1].Y;

            return point;
        }


        // Расчет контрольных точек
        private static Matrix StandartParameter(Point[] interpolationNodes)
        {
            // Основная матрица системы mainMatrix имеет размерность 2n x 2n, где 2n - количество контрольных точек. 
            // Поскольку на одну кривую приходится 2n контрольных точек, а количество кривых B равняется количество узлов интерполяции Q - 1.
            // Следовательно количество контрольных точек 2(Q - 1).
            int controlPointRange = 2 * (interpolationNodes.GetLength(0) - 1);

            Matrix mainMatrix = new Matrix(controlPointRange, controlPointRange);

            // Матрица свободных челенов freeMatrix.
            Matrix freeMatrix_X = new Matrix(controlPointRange, 1);
            Matrix freeMatrix_Y = new Matrix(controlPointRange, 1);

            // Первая строка mainMatrix согласно формулам 2A0 − B0 = Q0 имеет формат [2, -1, 0, 0 ... 0], а последняя согласно 2Bn−1 − An−1 = Qn [0 ... 0, 0, -1, 2].
            // Согласно этим формулам значения первой и последней строки freeMatrix будет [0, 0] и [interpolationNodes[maxIndex].X, interpolationNodes[maxIndex].Y] соответственно.
            mainMatrix[0, 0] = 2;
            mainMatrix[0, 1] = -1;
            mainMatrix[controlPointRange - 1, controlPointRange - 1] = 2;
            mainMatrix[controlPointRange - 1, controlPointRange - 2] = -1;
            freeMatrix_X[controlPointRange - 1, 0] = (decimal)(interpolationNodes[interpolationNodes.GetLength(0) - 1].X);
            freeMatrix_Y[controlPointRange - 1, 0] = (decimal)(interpolationNodes[interpolationNodes.GetLength(0) - 1].Y);

            // Остальные строки матрицы mainMatrix рекурсивно заполняются значениями 1, -2, 2, -1 и 0, 1, 1, 0 со смещением на [i + 2, j + 2] 
            // каждую иттерацию. Примечание: согласно формулам Ai−1 − 2Bi−1 + 2Ai − Bi = 0 и Bi−1 + Ai = 2Qi соответственно.
            // Массив freeMatrix будт заполняться значениями соответствующими interpolationNodes[maxIndex] умноженными на 2.
            // Индексация начинается с 2 строки и до предпоследней т.к. они уже заполнены.
            for (int i = 1, inpIndex = 1; i < controlPointRange - 2; i += 2, inpIndex++)
            {
                mainMatrix[i, i - 1] = 1;
                mainMatrix[i, i + 0] = -2;
                mainMatrix[i, i + 1] = 2;
                mainMatrix[i, i + 2] = -1;
                mainMatrix[i + 1, i + 0] = 1;
                mainMatrix[i + 1, i + 1] = 1;

                freeMatrix_X[i + 1, 0] = (decimal)(2 * interpolationNodes[inpIndex].X);
                freeMatrix_Y[i + 1, 0] = (decimal)(2 * interpolationNodes[inpIndex].Y);
            }

            // Решение систем относительно Х и Y(нахождение Х и Y координат контрольных точек соответственно)
            freeMatrix_X = Matrix.SLAE(mainMatrix, freeMatrix_X);
            freeMatrix_Y = Matrix.SLAE(mainMatrix, freeMatrix_Y);

            // Упаковка значений из freeMatrix_X и freeMatrix_Y в структуру Point
            Matrix point = new Matrix(controlPointRange, 2);

            for (int i = 0; i < controlPointRange; i++)
            {
                point[i, 0] = freeMatrix_X[i, 0];
                point[i, 1] = freeMatrix_Y[i, 0];
            }

            return point;
        }

    }

    /// <summary>
    /// Предоставляет методы для работы с матрицами и решения СЛАУ матричным способом.
    /// </summary>
    public class Matrix
    {
        private decimal[,] _array;


        // Индексатор
        public decimal this[int i, int j]
        {
            get
            {
                return _array[i, j];
            }

            set
            {
                if ((i > _array.GetLength(0) - 1) || (j > _array.GetLength(1) - 1))
                {
                    throw new IndexOutOfRangeException("Индекс находится вне границ матрицы.");
                }

                _array[i, j] = Convert.ToDecimal(value);
            }
        }


        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Matrix"/> размерности [1, 1].
        /// </summary>
        public Matrix()
        {
            _array = new decimal[1, 1];
        }


        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Matrix"/> размерности [i, j].
        /// </summary>
        public Matrix(int i, int j)
        {
            if ((i == 0) || (j == 0))
            {
                throw new ArgumentException("Одно из измерений переданного в метод массива равняется 0.");
            }

            _array = new decimal[i, j];
        }


        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Matrix"/> равного по размеру и содержанию переданному массиву.
        /// </summary>
        public Matrix(decimal[,] array)
        {
            if ((array.GetLength(0) == 0) || (array.GetLength(1) == 0))
            {
                throw new ArgumentException("По крайней мере одно из измерений переданного в метод массива равняется 0.");
            }

            _array = new decimal[array.GetLength(0), array.GetLength(1)];

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    _array[i, j] = array[i, j];
                }
            }
        }


        // Оператор "*".
        public static Matrix operator *(Matrix matrix, decimal factor)
        {
            Matrix temp = new Matrix(matrix.GetLength(0), matrix.GetLength(1));

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    temp[i, j] = matrix[i, j] * factor;
                }
            }

            return temp;
        }


        // Оператор "*".
        public static Matrix operator *(Matrix matrix_1, Matrix matrix_2)
        {
            if (matrix_1.GetLength(1) != matrix_2.GetLength(0)) // Если количество столбцов matrix_1 не равно количеству строк matrix_2, умножение не возможно
            {
                throw new ArgumentException("Rоличество столбцов matrix_1 не равно количеству строк matrix_2.");
            }

            Matrix temp = new Matrix(matrix_1.GetLength(0), matrix_2.GetLength(1));

            // Первые два цикла переберают все элементы массива
            for (int target_i = 0; target_i < temp.GetLength(0); target_i++)    // target_i - строка текущего элемента
            {
                for (int target_j = 0; target_j < temp.GetLength(1); target_j++)    // target_j - колонка текущего элемента
                {
                    // Третий цикл суммирует произведения соответствющих элементов target_i строки matrix_1 и target_j колонки matrix_2
                    decimal result = 0;

                    for (int i = 0; i < temp.GetLength(0); i++)
                    {
                        result += matrix_1[target_i, i] * matrix_2[i, target_j];
                        //result += matrix_1[i, target_j] * matrix_2[target_i, i];
                    }

                    temp[target_i, target_j] = result;
                }
            }

            return temp;

        }


        // Оператор "+".
        public static Matrix operator +(Matrix matrix_1, Matrix matrix_2)
        {
            if ((matrix_1.GetLength(0) != matrix_2.GetLength(0)) || (matrix_1.GetLength(1) != matrix_2.GetLength(1)))
            {
                throw new ArgumentException("Сложение матриц разной размерности.");
            }

            Matrix temp = new Matrix(matrix_1.GetLength(0), matrix_1.GetLength(1));

            for (int i = 0; i < matrix_1.GetLength(0); i++)
            {
                for (int j = 0; j < matrix_1.GetLength(1); j++)
                {
                    temp[i, j] = matrix_1[i, j] + matrix_2[i, j];
                }
            }

            return temp;
        }


        /// <summary>
        /// Решение системы линейных алгебраических уравнений.
        /// </summary>
        /// <param name="A">Основная матрица системы.</param>
        /// <param name="B">Матрица свободных членов.</param>
        public static Matrix SLAE(Matrix A, Matrix B)
        {
            return A.Invert() * B;
        }

        /// <summary>
        /// Возвращает обратную матрицу <see cref="Matrix"/>. 
        /// </summary>
        public Matrix Invert()
        {
            Matrix copy = new Matrix(_array);

            copy = copy.AlgebraicComplementMatrix();
            copy.Transpose();
            copy = copy * (1 / (new Matrix(_array).Determinant()));

            return copy;
        }


        /// <summary>
        /// Получает определитель матрицы <see cref="Matrix"/>.
        /// </summary>       
        public decimal Determinant()
        {
            // Определитель находится методом Гаусса

            if (_array.GetLength(0) != _array.GetLength(1))
            {
                throw new MemberAccessException("Невозможно вычислить определитель матрицы если она имеет не квадратный вид.");
            }

            // Определитель матрицы первого порядка есть сама матрица

            if (_array.GetLength(0) == 1)
            {
                return _array[0, 0];
            }

            decimal[,] temp = new decimal[_array.GetLength(0), _array.GetLength(1)];
            Array.Copy(_array, temp, _array.GetLength(0) * _array.GetLength(1));  // Локальная копия массива создается для того, что бы не изменять переданный массив

            int s = 1; // Коэффициент оприделяющий знак определителя

            // Приведение матрицы к треугольному виду 
            for (int i = 0; i < temp.GetLength(0) - 1; i++)    // (n - i) - порядок обрабатываемой матрицы
            {
                for (int j = i; j < temp.GetLength(0) - 1; j++)    // j - строки обрабатываемой матрицы
                {
                    // Исключение нулей в главной диагонали
                    if (temp[i, i] == 0)
                    {
                        int d = 1;

                        while (true)    // Поиск ненулевого элемента ряда (i + d)
                        {
                            if (d + i > temp.GetLength(0) - 1)    // Если поиск вышел за границы массива, то ряд нулевой => вернуть 0 
                            {
                                return 0;
                            }

                            if (temp[i, i + d] != 0)
                            {
                                break;
                            }

                            d++;
                        }

                        // Обмен местами колонки m и n в массиве temp

                        decimal tempSwap;

                        for (int k = 0; k < temp.GetLength(0); k++)
                        {
                            tempSwap = temp[k, i + d];
                            temp[k, i + d] = temp[k, i];
                            temp[k, i] = tempSwap;
                        }

                        s = s * -1;
                    }

                    // Приведение j + 1 строки к виду [0, Х, Х ... Х]
                    if (temp[j + 1, i] == 0)  // если следующий элемент столбца и так 0, то приведение не требуются
                    {
                        continue;
                    }

                    decimal divider = temp[j + 1, i] / temp[i, i];

                    for (int k = i; k < temp.GetLength(0); k++)   // k - элементы обрабатываемой строки
                    {
                        temp[j + 1, k] = temp[j + 1, k] - (temp[i, k] * divider);
                    }
                }
            }

            // Вычисление произведения главной диагонали
            decimal determinant = 1;

            for (int i = 0; i < temp.GetLength(0); i++)
            {
                determinant = (temp[i, i] * determinant);
            }

            return s * determinant;
        }


        /// <summary>
        /// Получает минор элемента [i, j] матрицы <see cref="Matrix"/>. 
        /// </summary>
        public decimal GetMinor(int i, int j)
        {
            if ((i > _array.GetLength(0) - 1) || (j > _array.GetLength(1) - 1))
            {
                throw new IndexOutOfRangeException("Индекс находился вне границ матрицы.");
            }

            if (_array.GetLength(0) != _array.GetLength(1))
            {
                throw new MemberAccessException("Невозможно вычислить минор матрицы если она имеет не квадратный вид.");
            }

            if (_array.GetLength(0) == 1)
            {
                throw new MemberAccessException("Невозможно вычислить минор матрицы первого порядка.");
            }

            decimal[,] temp = new decimal[_array.GetLength(0) - 1, _array.GetLength(1) - 1];

            // Циклы переберают все элементы массива за исключением элемента соответствующего позиции (target_i, target_j)
            for (int k = 0, matrixTemp_i = 0; k < _array.GetLength(0); k++)
            {
                if (k == i)
                {
                    continue;
                }

                for (int l = 0, matrixTemp_j = 0; l < _array.GetLength(1); l++)
                {
                    if (l == j)
                    {
                        continue;
                    }

                    temp[matrixTemp_i, matrixTemp_j] = _array[k, l];

                    matrixTemp_j++;
                }

                matrixTemp_i++;
            }

            return new Matrix(temp).Determinant();
        }


        /// <summary>
        /// Получает алгебраическое дополнение элемента [i, j], матрицы <see cref="Matrix"/>. 
        /// </summary>
        public decimal AlgebraicComplement(int i, int j)
        {
            if ((i > _array.GetLength(0) - 1) || (j > _array.GetLength(1) - 1))
            {
                throw new IndexOutOfRangeException("Индекс находился вне границ матрицы.");
            }

            return (new Matrix(_array).GetMinor(i, j)) * Convert.ToDecimal(Math.Pow(-1, ((i + 1) + (j + 1))));
        }


        /// <summary>
        /// Получает матрицу алгебраических дополнений матрицы <see cref="Matrix"/>. 
        /// </summary>
        public Matrix AlgebraicComplementMatrix()
        {


            Matrix temp = new Matrix(_array.GetLength(0), _array.GetLength(1));
            Matrix copy = new Matrix(_array);

            for (int i = 0; i < temp.GetLength(0); i++)
            {
                for (int j = 0; j < temp.GetLength(1); j++)
                {
                    temp[i, j] = copy.AlgebraicComplement(i, j);
                }
            }

            return temp;
        }


        /// <summary>
        /// Транспонирование матрицы <see cref="Matrix"/>. 
        /// </summary>
        /// <param name="matrix">Двумерный массив, представляющий собой матрицу <see cref="Matrix"/>.</param>
        public void Transpose()
        {
            decimal[,] temp = new decimal[_array.GetLength(1), _array.GetLength(0)];

            for (int i = 0; i < _array.GetLength(0); i++)    // i - строки обрабатываемой матрицы
            {
                for (int j = 0; j < _array.GetLength(1); j++)    // j - элементы строки обрабатываемой матрицы
                {
                    temp[j, i] = _array[i, j];
                }
            }

            _array = temp;
        }


        /// <summary>
        /// Получает 32-разрядное число, представляющее количество элементов в заданном измерении матрицы <see cref="Matrix"/>. 
        /// </summary>
        /// <param name="dimension">Измерение матрицы <see cref="Matrix"/>, индексация которой начинается с нуля, и для которого необходимо определить длинну.</param>
        public int GetLength(int dimension)
        {
            if (dimension == 0)
            {
                return _array.GetLength(0);
            }
            else if (dimension == 1)
            {
                return _array.GetLength(1);
            }
            else
            {
                throw new ArgumentException("Не существующее измерение матрицы.");
            }
        }


        /// <summary>
        /// Получает массив <see cref="decimal"/> из матрицы <see cref="Matrix"/>. 
        /// </summary>
        public decimal[,] GetArray(Matrix matrix)
        {
            return _array;
        }


        /// <summary>
        /// Меняет местами строки m и n в массиве <see cref="Matrix"/>. 
        /// </summary>
        public void SwapArrayRow(int m, int n)
        {
            if ((m > _array.GetLength(0) - 1) || (n > _array.GetLength(0) - 1))
            {
                throw new IndexOutOfRangeException("Индекс находился вне границ матрицы.");
            }

            if (m > _array.GetLength(1) - 1 || n > _array.GetLength(1) - 1)
            {
                return;
            }

            decimal temp;

            for (int i = 0; i < _array.GetLength(0); i++)
            {
                temp = _array[n, i];
                _array[n, i] = _array[m, i];
                _array[m, i] = temp;
            }
        }


        /// <summary>
        /// Меняет местами колонки m и n в массиве <see cref="Matrix"/>. 
        /// </summary>
        private void SwapArrayColumn(int m, int n)
        {
            if ((m > _array.GetLength(1) - 1) || (n > _array.GetLength(1) - 1))
            {
                throw new IndexOutOfRangeException("Индекс находился вне границ матрицы.");
            }

            if (m > _array.GetLength(0) - 1 || n > _array.GetLength(0) - 1)
            {
                return;
            }

            decimal temp;

            for (int i = 0; i < _array.GetLength(0); i++)
            {
                temp = _array[i, n];
                _array[i, n] = _array[i, m];
                _array[i, m] = temp;
            }
        }

    }
}
