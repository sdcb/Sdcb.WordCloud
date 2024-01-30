namespace Sdcb.WordClouds;

/// <summary>
/// Word and score pair
/// </summary>
/// <param name="Word">word ordered by occurance</param>
/// <param name="Score">score of the word, usually the occurance count</param>
public record WordScore(string Word, int Score);