using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Interfaces;
using Todo.Models;
using Todo.Parameters;

namespace Todo.Services
{
    public class AsyncService
    {
        public AsyncService()
        {

        }

        public async Task<int> 主作業()
        {
            var 作業1結果 = 作業1Async();
            var 作業2結果 = 作業2Async();

            var onetwo = await 作業1結果 + await 作業2結果;//同時下去執行 所以第一秒作業結果1執行完 作業2在第二秒就會執行玩了

            var 作業3結果 = 作業3Async(onetwo);

            int result = await 作業3結果;

            return result;
        }


        private async Task<int> 作業1Async()
        {
            await Task.Delay(1000);

            return 1;
        }
        private async Task<int> 作業2Async()
        {
            await Task.Delay(2000);

            return 2;
        }
        private async Task<int> 作業3Async(int i)
        {
            await Task.Delay(3000);

            return 3 * i;
        }
    }
}
