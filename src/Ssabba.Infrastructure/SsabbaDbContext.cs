using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure;

public class SsabbaDbContext(DbContextOptions<SsabbaDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerContact> PlayerContacts => Set<PlayerContact>();
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<CommunityInvite> CommunityInvites => Set<CommunityInvite>();
    public DbSet<CommunityLink> CommunityLinks => Set<CommunityLink>();
    public DbSet<JoinRequest> JoinRequests => Set<JoinRequest>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<CourtReservation> CourtReservations => Set<CourtReservation>();
    public DbSet<Format> Formats => Set<Format>();
    public DbSet<RuleSet> RuleSets => Set<RuleSet>();
    public DbSet<SessionTemplate> SessionTemplates => Set<SessionTemplate>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchSet> MatchSets => Set<MatchSet>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentEntry> TournamentEntries => Set<TournamentEntry>();
    public DbSet<MatchAppearance> MatchAppearances => Set<MatchAppearance>();
    public DbSet<MatchDispute> MatchDisputes => Set<MatchDispute>();
    public DbSet<PlayerFormatStat> PlayerFormatStats => Set<PlayerFormatStat>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<DuesPlan> DuesPlans => Set<DuesPlan>();
    public DbSet<DuesAssignment> DuesAssignments => Set<DuesAssignment>();
    public DbSet<FundingSource> FundingSources => Set<FundingSource>();
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
    public DbSet<EquipmentLoan> EquipmentLoans => Set<EquipmentLoan>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<WeatherObservation> WeatherObservations => Set<WeatherObservation>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<DataRequest> DataRequests => Set<DataRequest>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaSubject> MediaSubjects => Set<MediaSubject>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationOutbox> NotificationOutbox => Set<NotificationOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SsabbaDbContext).Assembly);
    }
}
