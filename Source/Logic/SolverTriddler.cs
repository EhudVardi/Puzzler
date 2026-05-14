using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic.Griddler;
using System.Collections;
using Common.Models.Base;
using Common.Models.Triddler;
using Common.Models.Griddler;

namespace Logic
{
    public class SolverTriddler : SolverGeneric<BoardTriddler>
    {

        class GroupVariations
        {
            public GroupGriddler Group { get; set; }
            public int VariationCount { get; set; }

            public GroupVariations(GroupGriddler group, int variationCount)
            {
                this.Group = group;
                this.VariationCount = variationCount;
            }
        }

        private HashSet<GroupGriddler> _oversizedGroups = new();
        private Dictionary<GroupGriddler, int> _oversizedGroupLastSetCount = new();
        private HashSet<GroupGriddler> _needsPropagation = new();
        private int _stepCount = 0;

        private const int INITIATION_THRESHOLD = 50_000;
        private const int RETRY_CELL_DELTA = 5;

        public override void SolveInitiation()
        {
            var t0 = DateTime.Now;
            _oversizedGroups = new HashSet<GroupGriddler>();
            _oversizedGroupLastSetCount = new Dictionary<GroupGriddler, int>();

            var groupsWithRawCount = Board.Groups
                .Select(g => new GroupVariations(g, CalcValidVariationsCount(g, INITIATION_THRESHOLD + 1)))
                .OrderBy(gv => gv.VariationCount)
                .ToList();

            _groupsVariations = Board.Groups.ToDictionary(g => (GroupGriddler)g, _ => new List<BitArray>());
            _needsPropagation = new HashSet<GroupGriddler>();

            int progress = 0;
            foreach (var gv in groupsWithRawCount)
            {
                if (gv.VariationCount > INITIATION_THRESHOLD)
                {
                    _oversizedGroups.Add(gv.Group);
                    ApplyConstrainedOverlap(gv.Group);
                    _oversizedGroupLastSetCount[gv.Group] = gv.Group.Cells.Count(c => c.Value != null);
                }
                else
                {
                    var vars = CalcAllValidVariationConsideringExsitingLine(gv.Group);
                    _groupsVariations[gv.Group] = vars;
                    ReflectIntegratedVariationToCells(gv.Group, vars);
                    _needsPropagation.Add(gv.Group);
                }
                ReportProgress(progress++, null);
            }

            Console.WriteLine($"[diag] SolveInitiation done in {(DateTime.Now-t0).TotalMilliseconds:F0}ms, oversized={_oversizedGroups.Count}");
        }

        public override bool DoCompleteStep()
        {
            var _sw = System.Diagnostics.Stopwatch.StartNew();
            int totalBefore = 0;
            foreach (var kvp in _groupsVariations) totalBefore += kvp.Value.Count;
            int setCellsBefore = Board.ValueCells.Count(c => c.Value != null);

            // Oversized groups: fast constrained overlap.
            var invalidated = new List<GroupGriddler>();
            foreach (var group in _oversizedGroups)
            {
                if (!ApplyConstrainedOverlap(group))
                    invalidated.Add(group);
            }
            foreach (var g in invalidated)
            {
                _oversizedGroups.Remove(g);
                _groupsVariations[g] = new List<BitArray>();
            }

            // Normal groups: propagate AND/OR → cells, but only for groups marked dirty
            // (whose variation list was pruned last step, or first call).
            foreach (GroupGriddler group in this.Board.Groups)
            {
                if (_oversizedGroups.Contains(group)) continue;
                if (!_needsPropagation.Contains(group)) continue;
                if (_groupsVariations[group].Count > 1 || group.Cells.Any(c => c.Value == null))
                    ReflectIntegratedVariationToCells(group, _groupsVariations[group]);
            }
            _needsPropagation.Clear();

            if (_stepCount <= 20) Console.WriteLine($"  s{_stepCount} propagate={_sw.ElapsedMilliseconds}ms");

            // Prune each normal group's variation list against current cells; mark dirty if changed.
            foreach (GroupGriddler group in this.Board.Groups)
            {
                if (_oversizedGroups.Contains(group)) continue;
                int before = _groupsVariations[group].Count;
                ReflectCellsToVariationsList(group);
                if (_groupsVariations[group].Count < before)
                    _needsPropagation.Add(group);
            }

            if (_stepCount <= 20) Console.WriteLine($"  s{_stepCount} prune={_sw.ElapsedMilliseconds}ms dirty={_needsPropagation.Count}");

            // Retry converting oversized groups — use a fast no-alloc count as pre-check.
            var resolved = new List<GroupGriddler>();
            foreach (var group in _oversizedGroups)
            {
                int currentSet = group.Cells.Count(c => c.Value != null);
                int lastSet = _oversizedGroupLastSetCount.GetValueOrDefault(group, 0);
                if (currentSet - lastSet < RETRY_CELL_DELTA) continue;

                // Fast count first (no BitArray allocation) — skip enumeration if still oversized.
                int fastCount = CalcConstrainedVariationsCountFast(group, INITIATION_THRESHOLD);
                _oversizedGroupLastSetCount[group] = currentSet;
                if (fastCount > INITIATION_THRESHOLD) continue;

                var vars = CalcAllValidVariationConsideringExsitingLine(group);
                _groupsVariations[group] = vars;
                ReflectIntegratedVariationToCells(group, vars);
                _needsPropagation.Add(group);
                resolved.Add(group);
            }
            foreach (var g in resolved) _oversizedGroups.Remove(g);

            int totalAfter = 0;
            foreach (var kvp in _groupsVariations) totalAfter += kvp.Value.Count;
            int setCellsAfter = Board.ValueCells.Count(c => c.Value != null);

            _stepCount++;
            if (_stepCount <= 20 || _stepCount % 100 == 0)
                Console.WriteLine($"[diag] step {_stepCount}: vars {totalBefore}->{totalAfter}, cells {setCellsBefore}->{setCellsAfter}, oversized={_oversizedGroups.Count}, t={_sw.ElapsedMilliseconds}ms");

            return totalAfter < totalBefore || setCellsAfter > setCellsBefore
                || resolved.Count > 0 || invalidated.Count > 0;
        }
        public override bool IsSolved()
        {
            foreach (CellValueGriddler valueCell in this.Board.ValueCells)
                if (valueCell.Value == null)
                    return false;

            return true;
        }
        public override bool IsValid()
        {
            foreach (var kvp in _groupsVariations)
                if (kvp.Value.Count == 0 && !_oversizedGroups.Contains(kvp.Key))
                    return false;
            return true;
        }
        public override void Reset() { }

        public Dictionary<GroupGriddler, List<BitArray>> _groupsVariations = null!;



        ///
        private List<BitArray> CalcAllValidVariationConsideringExsitingLine(GroupGriddler group)
        {
            List<BitArray> lines = new List<BitArray>();

            BitArray templateLine = new BitArray(group.Size);
            templateLine.SetAll(false);

            bool?[] existingLine = new bool?[group.Cells.Count];
            for (int i = 0; i < existingLine.Length; i++)
                existingLine[i] = group.Cells[i].Value;

            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, lines, templateLine, group.Cells, existingLine);

            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, List<BitArray> lines, BitArray currentLine, List<CellValueGriddler> cells, bool?[] existingLine)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                {
                    minCellsCount += nums[i] + 1;
                }
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    BitArray aLine = new BitArray(currentLine);

                    bool breaked = false;
                    //paint the current num on the relevant cells
                    int lastIndex = 0;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                    {
                        int index = start + j + i;
                        lastIndex = index;
                        aLine.Set(index, true);

                        if (existingLine[index].HasValue && existingLine[index].GetValueOrDefault() != aLine[index])
                        { breaked = true; break; }
                    }
                    if (breaked) 
                        continue;

                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, lines, aLine, cells, existingLine);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    bool? xor = CellValueGriddler.XOR(currentLine[i], cells[i].Value);
                    if (xor.HasValue && xor.Value == true)
                        return;
                }

                lines.Add(currentLine);
            }

        }


        private List<BitArray> CalcAllValidVariations(GroupGriddler group)
        {
            List<BitArray> lines = new List<BitArray>();

            BitArray templateLine = new BitArray(group.Size);
            templateLine.SetAll(false);

            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, lines, templateLine, group.Cells);

            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, List<BitArray> lines, BitArray currentLine, List<CellValueGriddler> cells)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                {
                    minCellsCount += nums[i] + 1;
                }
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    //clone the line for the recursive call
                    BitArray aLine = new BitArray(currentLine.Length);
                    for (int k = 0; k < currentLine.Count; k++)
                    {
                        aLine.Set(k, currentLine[k]);
                    }

                    //paint the current num on the relevant cells
                    int lastIndex = 0;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                    {
                        int index = start + j + i;
                        lastIndex = index;
                        aLine.Set(index, true);
                    }

                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, lines, aLine, cells);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    bool? xor = CellValueGriddler.XOR(currentLine[i], cells[i].Value);
                    if (xor.HasValue && xor.Value == true)
                        return;
                }

                lines.Add(currentLine);
            }

        }

        private int CalcValidVariationsCount(GroupGriddler group, int maxCount = int.MaxValue)
        {
            int lines = 0;
            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, ref lines, maxCount);
            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, ref int lines, int maxCount)
        {
            if (lines > maxCount) return;
            if (currentNumI < nums.Count - 1)
            {
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                    minCellsCount += nums[i] + 1;
                minCellsCount -= 2;

                int gap = n - minCellsCount - start;
                for (int i = 0; i < gap; i++)
                {
                    if (lines > maxCount) return;
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, ref lines, maxCount);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                lines++;
            }
        }

        // Counts constrained variations without allocating BitArrays. Stops early at maxCount+1.
        // Slightly optimistic (may overcount) but safe to use as a cheap pre-check.
        private int CalcConstrainedVariationsCountFast(GroupGriddler group, int maxCount)
        {
            bool?[] existing = new bool?[group.Cells.Count];
            for (int i = 0; i < existing.Length; i++) existing[i] = group.Cells[i].Value;
            int count = 0;
            CalcConstrainedVarsCountRec(group.Size, group.Numbers, -1, 0, existing, ref count, maxCount);
            return count;
        }

        private void CalcConstrainedVarsCountRec(int n, List<int> nums, int currentNumI, int start,
            bool?[] existing, ref int count, int maxCount)
        {
            if (count > maxCount) return;
            if (currentNumI < nums.Count - 1)
            {
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                    minCellsCount += nums[i] + 1;
                minCellsCount -= 2;
                int gap = n - minCellsCount - start;
                for (int i = 0; i < gap; i++)
                {
                    if (count > maxCount) return;
                    bool skip = false;
                    int bs = start + i;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                        if (existing[bs + j] == false) { skip = true; break; }
                    if (!skip)
                        CalcConstrainedVarsCountRec(n, nums, currentNumI + 1,
                            bs + nums[currentNumI + 1] + 1, existing, ref count, maxCount);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                count++;
            }
        }

        // Generates variations constrained by existing cells, stopping early if count > maxCount.
        // Returns null when the limit is exceeded.
        private List<BitArray>? CalcAllValidVariationConsideringExistingWithLimit(GroupGriddler group, int maxCount)
        {
            var lines = new List<BitArray>();
            var template = new BitArray(group.Size);
            template.SetAll(false);

            bool?[] existingLine = new bool?[group.Cells.Count];
            for (int i = 0; i < existingLine.Length; i++)
                existingLine[i] = group.Cells[i].Value;

            bool tooMany = false;
            CalcAllValidVariationsRecursiveWithLimit(group.Size, group.Numbers, -1, 0, lines, template, group.Cells, existingLine, maxCount, ref tooMany);
            return tooMany ? null : lines;
        }

        private void CalcAllValidVariationsRecursiveWithLimit(int n, List<int> nums, int currentNumI, int start,
            List<BitArray> lines, BitArray currentLine, List<CellValueGriddler> cells, bool?[] existingLine,
            int maxCount, ref bool tooMany)
        {
            if (tooMany) return;
            if (currentNumI < nums.Count - 1)
            {
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                    minCellsCount += nums[i] + 1;
                minCellsCount -= 2;

                int gap = n - minCellsCount - start;
                for (int i = 0; i < gap; i++)
                {
                    if (tooMany) return;
                    BitArray aLine = new BitArray(currentLine);
                    bool breaked = false;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                    {
                        int index = start + j + i;
                        aLine.Set(index, true);
                        if (existingLine[index].HasValue && existingLine[index].GetValueOrDefault() != aLine[index])
                        { breaked = true; break; }
                    }
                    if (breaked) continue;
                    CalcAllValidVariationsRecursiveWithLimit(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1,
                        lines, aLine, cells, existingLine, maxCount, ref tooMany);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    bool? xor = CellValueGriddler.XOR(currentLine[i], cells[i].Value);
                    if (xor.HasValue && xor.Value == true) return;
                }
                lines.Add(currentLine);
                if (lines.Count > maxCount) tooMany = true;
            }
        }

        // Constrained overlap with iterative TRUE-cell propagation.
        // Maintains per-block lower/upper start bounds; iterates forward (leftmost placement
        // avoiding FALSE cells) + backward (rightmost) + TRUE-cell pass (if only one block
        // can cover a TRUE cell, tighten that block's bounds) until stable.
        // Returns false when no valid arrangement exists.
        private bool ApplyConstrainedOverlap(GroupGriddler group)
        {
            int n = group.Size;
            var nums = group.Numbers;

            if (nums.Count == 0)
            {
                for (int p = 0; p < n; p++)
                    if (group.Cells[p].Value == null)
                        group.Cells[p].Value = false;
                return true;
            }

            int m = nums.Count;
            int[] leftStart = new int[m];
            int[] rightStart = new int[m];
            int[] minBound = new int[m];
            int[] maxBound = new int[m];

            // Initial ordering bounds
            minBound[0] = 0;
            for (int i = 1; i < m; i++)
                minBound[i] = minBound[i - 1] + nums[i - 1] + 1;
            maxBound[m - 1] = n - nums[m - 1];
            for (int i = m - 2; i >= 0; i--)
                maxBound[i] = maxBound[i + 1] - nums[i] - 1;

            for (int i = 0; i < m; i++)
                if (minBound[i] > maxBound[i]) return false;

            bool changed = true;
            while (changed)
            {
                changed = false;

                // Forward pass: leftmost valid start for each block (skips FALSE cells)
                for (int i = 0; i < m; i++)
                {
                    int s = FindLeftmostStart(group, minBound[i], nums[i]);
                    if (s < 0 || s > maxBound[i]) return false;
                    if (s != leftStart[i]) { leftStart[i] = s; changed = true; }
                    if (i + 1 < m)
                    {
                        int nextMin = s + nums[i] + 1;
                        if (nextMin > minBound[i + 1]) { minBound[i + 1] = nextMin; changed = true; }
                    }
                }

                // Backward pass: rightmost valid start for each block
                for (int i = m - 1; i >= 0; i--)
                {
                    int s = FindRightmostStart(group, maxBound[i], nums[i]);
                    if (s < 0 || s < leftStart[i]) return false;
                    if (s != rightStart[i]) { rightStart[i] = s; changed = true; }
                    if (i > 0)
                    {
                        int prevMax = s - nums[i - 1] - 1;
                        if (prevMax < maxBound[i - 1]) { maxBound[i - 1] = prevMax; changed = true; }
                    }
                }

                // TRUE-cell pass: if only one block can cover a TRUE cell, tighten its bounds
                for (int p = 0; p < n; p++)
                {
                    if (group.Cells[p].Value != true) continue;

                    int coverCount = 0, soleBlock = -1;
                    for (int i = 0; i < m; i++)
                        if (p >= leftStart[i] && p <= rightStart[i] + nums[i] - 1)
                        { coverCount++; if (soleBlock < 0) soleBlock = i; }

                    if (coverCount == 0) return false;
                    if (coverCount == 1)
                    {
                        int i = soleBlock;
                        // Block must start at or before p to cover it
                        if (p < maxBound[i]) { maxBound[i] = p; changed = true; }
                        // Block must end at or after p, so start >= p - len + 1
                        int newMin = p - nums[i] + 1;
                        if (newMin > minBound[i]) { minBound[i] = newMin; changed = true; }
                    }
                }
            }

            // Verify all TRUE cells are covered
            for (int p = 0; p < n; p++)
            {
                if (group.Cells[p].Value != true) continue;
                bool covered = false;
                for (int i = 0; i < m; i++)
                    if (p >= leftStart[i] && p <= rightStart[i] + nums[i] - 1)
                    { covered = true; break; }
                if (!covered) return false;
            }

            // Definite fills: overlap of leftmost and rightmost placement per block
            for (int i = 0; i < m; i++)
                for (int p = rightStart[i]; p <= leftStart[i] + nums[i] - 1; p++)
                    if (group.Cells[p].Value == null)
                        group.Cells[p].Value = true;

            // Definite empties: cells outside every block's reachable range
            for (int p = 0; p < n; p++)
            {
                if (group.Cells[p].Value != null) continue;
                bool reachable = false;
                for (int i = 0; i < m; i++)
                    if (p >= leftStart[i] && p <= rightStart[i] + nums[i] - 1)
                    { reachable = true; break; }
                if (!reachable)
                    group.Cells[p].Value = false;
            }

            return true;
        }

        // Finds the leftmost block start >= minStart with no FALSE cell in [start, start+len-1].
        private int FindLeftmostStart(GroupGriddler group, int minStart, int blockLen)
        {
            int pos = minStart;
            int n = group.Size;
            while (pos + blockLen <= n)
            {
                int rightmostFalse = -1;
                for (int j = blockLen - 1; j >= 0; j--)
                    if (group.Cells[pos + j].Value == false)
                    { rightmostFalse = pos + j; break; }
                if (rightmostFalse < 0) return pos;
                pos = rightmostFalse + 1;
            }
            return -1;
        }

        // Finds the rightmost block start <= maxStart with no FALSE cell in [start, start+len-1].
        private int FindRightmostStart(GroupGriddler group, int maxStart, int blockLen)
        {
            int pos = Math.Min(maxStart, group.Size - blockLen);
            while (pos >= 0)
            {
                int leftmostFalse = -1;
                for (int j = 0; j < blockLen; j++)
                    if (group.Cells[pos + j].Value == false)
                    { leftmostFalse = pos + j; break; }
                if (leftmostFalse < 0) return pos;
                pos = leftmostFalse - blockLen;
            }
            return -1;
        }

        private void ReflectIntegratedVariationToCells(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            GroupGriddler MPLine = GetIntegratedGroup(group, _groupsVariations);

            for (int i = 0; i < group.Size; i++)
                if (MPLine.Cells[i].Value != null)
                    group.Cells[i].Value = MPLine.Cells[i].Value;
        }

        private void ReflectCellsToVariationsList(GroupGriddler group)
        {
            List<BitArray> current = _groupsVariations[group];
            List<BitArray> survivors = new List<BitArray>(current.Count);

            foreach (BitArray variation in current)
            {
                bool valid = true;
                for (int j = 0; j < group.Size; j++)
                {
                    if (group.Cells[j].Value != null && variation.Get(j) != group.Cells[j].Value)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                    survivors.Add(variation);
            }

            _groupsVariations[group] = survivors;
        }



        private GroupGriddler GetIntegratedGroup(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            GroupGriddler l = new GroupGriddler(group.Size);

            bool?[] and = GetIntegratedAndArray(group, _groupsVariations);
            bool?[] or = GetIntegratedOrArray(group, _groupsVariations);

            for (int i = 0; i < group.Size; i++)
            {
                if (and[i] == null && or[i] == null)
                    l.Cells[i].Value = null;
                else if (and[i] == true)
                    l.Cells[i].Value = true;
                else if (or[i] == false)
                    l.Cells[i].Value = false;
                else
                    throw new Exception();
            }

            return l;

        }


        private bool?[] GetIntegratedOrArray(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            bool?[] ORLineArray = new bool?[group.Size];
            for (int i = 0; i < ORLineArray.Length; i++)
                ORLineArray[i] = false;

            List<BitArray> _multiplexedLines = _groupsVariations;

            for (int i = 0; i < ORLineArray.Length; i++)
                for (int j = 0; j < _multiplexedLines.Count; j++)
                    ORLineArray[i] = CellValueGriddler.OR(ORLineArray[i].GetValueOrDefault(), _multiplexedLines[j].Get(i));

            for (int i = 0; i < ORLineArray.Length; i++)
                if (ORLineArray[i] == true)
                    ORLineArray[i] = null;

            return ORLineArray;
        }

        private bool?[] GetIntegratedAndArray(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            bool?[] ANDLineArray = new bool?[group.Size];
            for (int i = 0; i < ANDLineArray.Length; i++)
                ANDLineArray[i] = true;

            List<BitArray> _multiplexedLines = _groupsVariations;

            for (int i = 0; i < ANDLineArray.Length; i++)
                for (int j = 0; j < _multiplexedLines.Count; j++)
                    ANDLineArray[i] = CellValueGriddler.AND(ANDLineArray[i].GetValueOrDefault(true), _multiplexedLines[j].Get(i));

            for (int i = 0; i < ANDLineArray.Length; i++)
                if (ANDLineArray[i] == false)
                    ANDLineArray[i] = null;

            return ANDLineArray;

        }


        ///

        private sealed record TriddlerSnapshot(
            Dictionary<GroupGriddler, List<BitArray>> Variations,
            Dictionary<CellValueGriddler, bool?> CellValues,
            HashSet<GroupGriddler> OversizedGroups,
            Dictionary<GroupGriddler, int> OversizedGroupLastSetCount,
            HashSet<GroupGriddler> NeedsPropagation);

        protected override object TakeSnapshot()
        {
            var variations = new Dictionary<GroupGriddler, List<BitArray>>();
            foreach (var kvp in _groupsVariations)
                variations[kvp.Key] = kvp.Value.Select(ba => new BitArray(ba)).ToList();

            var values = new Dictionary<CellValueGriddler, bool?>();
            foreach (CellValueGriddler cell in Board.ValueCells)
                values[cell] = cell.Value;

            return new TriddlerSnapshot(
                variations, values,
                new HashSet<GroupGriddler>(_oversizedGroups),
                new Dictionary<GroupGriddler, int>(_oversizedGroupLastSetCount),
                new HashSet<GroupGriddler>(_needsPropagation));
        }

        protected override void RestoreSnapshot(object snapshot)
        {
            var s = (TriddlerSnapshot)snapshot;
            foreach (var kvp in s.Variations)
                _groupsVariations[kvp.Key] = kvp.Value.Select(ba => new BitArray(ba)).ToList();
            foreach (var kvp in s.CellValues)
                kvp.Key.Value = kvp.Value;
            _oversizedGroups = new HashSet<GroupGriddler>(s.OversizedGroups);
            _oversizedGroupLastSetCount = new Dictionary<GroupGriddler, int>(s.OversizedGroupLastSetCount);
            _needsPropagation = new HashSet<GroupGriddler>(s.NeedsPropagation);
        }

        protected override IEnumerable<Action> GetBranches()
        {
            // Prefer branching on the enumerable group with fewest undecided variations.
            GroupGriddler? best = null;
            int bestCount = int.MaxValue;
            foreach (GroupGriddler group in this.Board.Groups)
            {
                if (_oversizedGroups.Contains(group)) continue;
                int count = _groupsVariations[group].Count;
                if (count > 1 && count < bestCount)
                {
                    best = group;
                    bestCount = count;
                }
            }

            if (best != null)
            {
                var candidates = new List<BitArray>(_groupsVariations[best]);
                foreach (BitArray variation in candidates)
                {
                    BitArray captured = variation;
                    GroupGriddler capturedGroup = best;
                    yield return () =>
                    {
                        _groupsVariations[capturedGroup] = new List<BitArray> { captured };
                        ReflectIntegratedVariationToCells(capturedGroup, _groupsVariations[capturedGroup]);
                    };
                }
                yield break;
            }

            // Fall back: branch on an unknown cell belonging to an oversized group.
            foreach (GroupGriddler group in _oversizedGroups)
            {
                CellValueGriddler? unknown = group.Cells.FirstOrDefault(c => c.Value == null);
                if (unknown != null)
                {
                    CellValueGriddler cell = unknown;
                    yield return () => cell.Value = true;
                    yield return () => cell.Value = false;
                    yield break;
                }
            }
        }
    }
}
