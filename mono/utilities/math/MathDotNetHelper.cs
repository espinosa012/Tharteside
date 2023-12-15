using MathNet.Numerics.LinearAlgebra;

namespace Tartheside.mono.utilities.math;

public static class MathDotNetHelper
{
    // Convolution
    public static Matrix<float> GetSobelMatrix(Matrix<float> input)
    {
        var kernel_sobelX = Matrix<float>.Build.DenseOfArray(new float[,] {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}});
        var kernel_sobelY = Matrix<float>.Build.DenseOfArray(new float[,] {{-1, -2, -1}, {0, 0, 0}, {1, 2, 1}});

        return Convolve(input, kernel_sobelX) + Convolve(input, kernel_sobelY);
    }
    private static Matrix<float> Convolve(Matrix<float> input, Matrix<float> kernel)
    {
        var m = input.RowCount;
        var n = input.ColumnCount;
        var km = kernel.RowCount;
        var kn = kernel.ColumnCount;

        var result = Matrix<float>.Build.Dense(input.RowCount, input.ColumnCount);

        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
        {
            float sum = 0;
            for (var ki = 0; ki < km; ki++)
            for (var kj = 0; kj < kn; kj++)
            {
                var ni = i + ki - km / 2;
                var nj = j + kj - kn / 2;
                if (ni >= 0 && ni < m && nj >= 0 && nj < n)
                    sum += input[ni, nj] * kernel[ki, kj];
            }
            result[i, j] = sum;
        }

        return result;
    }
    
}