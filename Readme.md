## HandyJobs

A collection of handy Unity jobs.

### Installation

In UPM, install from git URL:
> `git@github.com:martindevans/HandyJobs.git?path=/Packages/me.martindevans.handyjobs`

## Jobs

### Auction Algorithm

Assigns workers to jobs. Each worker has a value for doing each job, the auction algorithm performs a global optimisation across all workers. For example; assigning defensive towers to targets in an RTS. Workers may remain unassigned if none of the jobs are worth bidding for (forced when workers outnumber jobs, otherwise opt-in via `allowUnassignedAgents`).

### Prefix Sum

For each element in an array, calculates the sum of it with all previous elements. e.g. `[ 1, 2, 3, 4 ] => [ 1, 3, 6, 10 ]`