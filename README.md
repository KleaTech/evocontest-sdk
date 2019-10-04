# evocontest-sdk
SDK for the evosoft 2019 Miskolc programming contest. Contest site: [evocontest](https://evocontest.azurewebsites.net).

Solution structure:
- **Submission**
    - **evocontest.Submission:** Contains the user's solution for the contest.
    - evocontest.Submission.Sample: Contains a sample solution for the contest.
- Common
    - evocontest.Common: Dependencies for all projects.
- **Runner**
    - **evocontest.Submission.Runner:** Runs the user's submission, to test its performance.
    - evocontest.Runner.Common: Dependencies of evocontest.Submission.Runner.
- **Test**
    - **evocontest.Submission.Test:** Contains unit tests for both sample and user's submission.

To participate, implement a solution, and upload the *Release / AnyCPU* build of *evocontest.Submission* on the contest site.

Note: Only a single *ISolution* implementation is allowed in the uploaded dll.