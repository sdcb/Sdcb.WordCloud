namespace Sdcb.WordCloud;

/// <summary>
/// Word and frequency pair
/// </summary>
/// <param name="Word">word ordered by occurance</param>
/// <param name="Frequency">frequecy</param>
public record WordFrequency(string Word, int Frequency);