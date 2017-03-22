using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandsOnSharp
{
    class TaskParallelism
    {

        static int SeqentialImageProcessing(Bitmap source1, Bitmap source2, 
            Bitmap layer1, Bitmap layer2, Graphics blender)
        {
            SetToGray(source1, layer1);
            Rotate(source2, layer2);
            Blend(layer1, layer2, blender);
            return source1.Width;
        }
        static int ParallelTaskImageProcessing(Bitmap source1, Bitmap source2, 
            Bitmap layer1, Bitmap layer2, Graphics blender) {
            Task toGray = Task.Factory.StartNew(() => SetToGray(source1, layer1));
            Task rotate = Task.Factory.StartNew(() => Rotate(source2, layer2));
            Task.WaitAll(toGray, rotate);
            Blend(layer1, layer2, blender);
            return source1.Width;
        }

        static int ParallelInvokeImageProcessing(Bitmap source1, 
            Bitmap source2, Bitmap layer1, 
            Bitmap layer2, Graphics blender)
        {
            Parallel.Invoke(
                    () => SetToGray(source1, layer1), 
                    () => Rotate(source2, layer2)
            );

            var x = Task.Factory.StartNew(() => Task.Factory.StartNew(() => 1));
            x.Unwrap
            Blend(layer1, layer2, blender);
            return source1.Width;
        }

        private static void Blend(Bitmap layer1, Bitmap layer2, Graphics blender)
        {
            throw new NotImplementedException();
        }

        private static void Rotate(Bitmap source2, Bitmap layer2)
        {
            throw new NotImplementedException();
        }

        private static void SetToGray(Bitmap source1, Bitmap layer1)
        {
            throw new NotImplementedException();
        }
    }
}
