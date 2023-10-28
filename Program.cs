using Project_Carlo;
using NSMBRNG.NSMBLCG;

var input = Console.ReadLine();

var type2int = new Dictionary<char, ulong>();
type2int.Add('F', 0UL);
type2int.Add('K', 1UL);
type2int.Add('S', 2UL);
type2int.Add('C', 3UL);
type2int.Add('M', 4UL);
type2int.Add('L', 5UL);


var inputcode = 0UL;
var cardnum = 15;
for (int i = 0; i < cardnum; i++)
{
    inputcode <<= 3;
    inputcode |= type2int.GetValueOrDefault(input[i]);
}

// Stopwatchクラス生成
var sw = new System.Diagnostics.Stopwatch();
var ts = sw.Elapsed;
DateTime dt = DateTime.Now;
Console.WriteLine($"{dt} 開始");
sw.Start();
//-----------------
// 計測開始

Console.WriteLine("Carlo Search Start.");

const UInt32 SEARCHMAX = 0x33333333U;//mod 0x33333333で循環?
const UInt32 DIVCONST = 0x11;


var carloseed = 0U;

Parallel.For(0, DIVCONST, t =>
{
    uint divseed = (SEARCHMAX/DIVCONST) * (uint)t;
    for (uint i = 0; i < SEARCHMAX/DIVCONST; i++)
    {
        var cseed = i + divseed;

        var seed = cseed;
        var deck = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5 }.ToList();

        var handcode = 0UL;
        for (int j = 0; j < cardnum; j++)
        {
            var idx = (int)seed.GetRand((uint)deck.Count);
            var card = deck[idx];
            handcode <<= 3;
            handcode |= card;
            deck.RemoveAt(idx);
        }
        if (handcode == inputcode)
        {
            carloseed = cseed;
            break;
        }
    }
});

var seedset = new HashSet<UInt32>();
for (int i = 0; i < 5; i++)
{
    seedset.Add(carloseed);
    var isCarried = (((UInt64)carloseed) +((UInt64)SEARCHMAX))>0xFFFFFFFFUL;
    carloseed += SEARCHMAX + Convert.ToUInt32(isCarried);
    Console.WriteLine($"Found! CarloSeed:0x{carloseed.ToString("X8")}");
}
Console.WriteLine("Carlo Search Finished.");

Console.WriteLine("Initseed Search Start.");

//パラメータ検索
var timer0s = Enumerable.Range(0x00, 0x100).Select(i => Convert.ToUInt32(i)).ToArray();
var vcounts = Enumerable.Range(0x00, 0x100).Select(i => Convert.ToUInt32(i)).ToArray();


var mac = new uint[] { 0x00, 0x1a, 0xe9, 0x03, 0x56, 0xbe };
var gxstat = 0x86000000;//ミニゲーム
var mode = 0x1U;//ミニゲーム

var datecode = Misc.YYMMDDToDatecode(new uint[] { 0, 1, 1 });//起動日(年下二桁,月,日)
var timecodes = Enumerable.Range(0, 60).Select(i => Misc.HHMMSSToTimecode(new uint[] { 00, 00, (uint)i }));//"ミニゲーム"の選択時刻(00:00:00-00:00:59)
var vframe = 0x0007;
uint[] seeds = null;

foreach (var timecode in timecodes)
{
    var msg = Misc.CreateBaseMessage(gxstat, mode, (uint)vframe, mac, datecode, timecode);
    var sg = new SeedGenerator(msg);
    seeds = sg.calculate(timer0s, vcounts);

    for (int i=0;i<seeds.Length;i++)
    {
        var initseed = seeds[i];
        var seed = initseed;
        for (var j = 0; j < 180; j++)//消費数
        {
            if (seedset.Contains(seed))
            {
                var t0idx = i/vcounts.Length;
                var vcidx = i%vcounts.Length;
                var timer0 = timer0s[t0idx];
                var vcount = vcounts[vcidx];

                Console.WriteLine($"Found! InitSeed: 0x{initseed.ToString("X8")}, CarloSeed: 0x{seed.ToString("X8")}, Vframe: {vframe.ToString("X4")}, Timer0:0x{timer0.ToString("X4")}, VCount:{vcount.ToString("X4")}, advance:{j}, timecode:{timecode.ToString("X8")}");
                i = seeds.Length;
                break;
            }
            seed.Advance();
        }
    }
}


// 計測停止
sw.Stop();
Console.WriteLine($"　{sw.ElapsedMilliseconds} ms");
dt = DateTime.Now;
Console.WriteLine($"{dt} 終了");
