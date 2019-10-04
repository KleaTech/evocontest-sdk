# evocontest-sdk
SDK for the evosoft 2019 Miskolc programming contest. Contest site: [evocontest](https://evocontest.azurewebsites.net).

Solution structure:
- **Submission**
    - **evocontest.Submission:** Contains the user's solution for the contest.
    - evocontest.Submission.Sample: Contains a sample solution for the contest.
- **Runner**
    - **evocontest.Submission.Runner:** Runs the user's submission, to test its performance.
- **Test**
    - **evocontest.Submission.Test:** Contains unit tests for both sample and user's submission.
- Common
    - evocontest.Common: Dependencies for all projects.
    - evocontest.Runner.Common: Dependencies of evocontest.Submission.Runner.

To participate, implement a solution, and upload the *Release / AnyCPU* build of *evocontest.Submission* on the contest site.

Notes:
- Only a single *ISolution* implementation is allowed in the uploaded dll.
- *evocontest.Submission* should only ever depend on *evocontest.Common*.