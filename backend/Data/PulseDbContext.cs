using Microsoft.EntityFrameworkCore;
using Pulse.Api.Models;

namespace Pulse.Api.Data;

public class PulseDbContext(DbContextOptions<PulseDbContext> options) : DbContext(options)
{
    public DbSet<Library> Libraries => Set<Library>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<PlaybackProgress> PlaybackProgresses => Set<PlaybackProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlbumArtist>()
            .HasKey(aa => new { aa.AlbumId, aa.ArtistId });

        modelBuilder.Entity<AlbumArtist>()
            .HasOne(aa => aa.Album)
            .WithMany(a => a.AlbumArtists)
            .HasForeignKey(aa => aa.AlbumId);

        modelBuilder.Entity<AlbumArtist>()
            .HasOne(aa => aa.Artist)
            .WithMany(a => a.AlbumArtists)
            .HasForeignKey(aa => aa.ArtistId);

        modelBuilder.Entity<Artist>()
            .HasIndex(a => a.NormalizedName)
            .IsUnique();

        modelBuilder.Entity<Track>()
            .HasIndex(t => t.FilePath)
            .IsUnique();

        modelBuilder.Entity<Track>()
            .HasIndex(t => new { t.AlbumId, t.DiscNumber, t.TrackNumber });

        modelBuilder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(pt => pt.PlaylistId);

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany()
            .HasForeignKey(pt => pt.TrackId);

        modelBuilder.Entity<PlaybackProgress>()
            .HasKey(pp => pp.TrackId);

        modelBuilder.Entity<PlaybackProgress>()
            .HasOne(pp => pp.Track)
            .WithMany()
            .HasForeignKey(pp => pp.TrackId);
    }
}
