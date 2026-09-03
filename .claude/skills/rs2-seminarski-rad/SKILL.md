---
name: rs2-seminarski-rad
description: Rules and checklist for the group term/seminar project ("seminarski rad") in the Software Development 2 (RS2) course - agile process, microservices, GitHub Projects, sprints, testing, documentation, deadlines, and defense. Use this skill whenever the user mentions the seminar/term project, the RS2 course project, forming a team for it, proposing/registering a topic, sprints, microservices for a school/university project, defending the project, or asks for help planning, organizing, documenting, or submitting this specific project - even if they don't use these exact words (e.g. "help me plan my team's uni project", "how do I set up a GitHub Projects board for my school project", "what do I need for the defense"). Also use it when the user asks whether the team's current work meets the rules (e.g. number of sprints, team size, containerization, tests).
---

# Seminar/Term Project – RS2 (Software Development 2)

This skill summarizes the official rules for the group seminar/term project and turns them into a practical checklist. Use it to help the user plan a team, organize work through sprints, satisfy the technical requirements, and prepare for the defense — without missing any formal requirement.

When the user asks for help with this project, first figure out which phase they're in (forming the team / registering the topic / development / documentation / defense), then apply the relevant part of the checklist below. If a rule isn't clearly applicable to their situation, ask rather than assume.

## 1. Team

- Group work, **3–7 members** (exceptionally more).
- Each member must be responsible for developing **at least one microservice** — when helping split up the work, make sure everyone clearly owns a part.
- If a member drops the exam/project, **the remaining members must take over that person's part of the work** — the scope doesn't shrink because of it.
- All team members receive **the same single grade** for the seminar project.

## 2. Methodology and technologies

- Work is organized using **agile development methodologies** (Scrum/Kanban-style sprints).
- As a rule, use the technologies covered in the RS2 course. Other technologies/frameworks/delivery mechanisms are allowed **only with explicit permission** from the instructor and TA — if the user proposes an unusual stack, remind them to get approval first before relying on it.
- The solution must cover the elements from lectures/labs: **gRPC, DDD, CQRS, message queues, API gateways, security**, etc. — when reviewing the team's architecture, check whether these elements are present.
- The client application should be a **single-page application (SPA)** — e.g. Angular 12, React.
- The project **must be containerized** (recommended: Docker + Docker Compose; other alternatives are allowed too).

## 3. Testing

- Every subsystem/microservice → **unit tests**.
- The whole system → **integration or E2E tests**.
- When reviewing a plan or code, check that both testing levels exist, not just unit tests at the service level.

## 4. Code, repository, and tools

- The software is **open-source and publicly usable**, with documentation available.
- Use **GitHub** for project management (**GitHub Project Boards required**) and for team communication/collaboration.
- **Commits must be meaningful** — each member should commit their own changes (not "commit 1", "test commit", etc.) so real contributions are visible.
- When helping with commit messages or board organization, insist on this readability and traceability of ownership.

## 5. Sprints

- Minimum **4 sprints**.
- **Every team member must participate in every sprint.**
- A sprint should last roughly **one week to one month** (flexible due to students' other obligations).
- Every verbal team agreement should be **written into the sprint description** on the GitHub Project Board — this is a common thing teams forget, so flag it to the user.

## 6. Registration process and workflow

1. **Topic proposal** – an email to the instructor and TA with a short description of the idea; **all team members must be included** in the thread.
2. After the topic is approved (following email discussion), **a team representative registers the topic via a survey/form** published on the course website.
   - Deadline to register the topic: **by December 25, 2025.**
   - When registering, include: a short description of the idea/topic, the source code repository address, and the address of the repository/place where the team's interactive communication is kept.
   - Use GitHub Project Boards for project management.
   - Students without a team by this deadline will be assigned to an existing team or to a newly formed one.
3. The TA enters approved topics into a shared document with the list of students and grades (visible, view-only, to all students on the course that academic year).
4. Development – consultations with the TA/instructor via email, during breaks between classes, or during office hours; exceptionally also via Skype/Viber/WhatsApp.
5. When the work is done, **all team members together (in the same email)** notify the instructor and TA that it's finished — the defense date is then scheduled.
6. **Defenses take place during exam periods**, at times set by the exam schedule.

### Final submission deadline

- **September 1, 2026** is the final deadline to submit the seminar project for the 2025/26 academic year. If the user asks "will I make it in time", compare the current date to this deadline and give a realistic estimate of how many sprints/how much time they have left.

## 7. Documentation (required parts)

When helping with project documentation, check that all of these are present:

- User manual/guide.
- If the app isn't publicly deployed — instructions on how to set it up and run it locally.
- Documented code (e.g. Doxygen for ASP.NET, Compodoc for Angular, or the equivalent tool for the stack used).
- Diagrams of the application's subsystems (minimum: a class diagram for each subsystem).
- The application should be **deployed/published**.

## 8. Defense

- **All team members** defend, in this order:
  1. Presentation of the developed software (whole team).
  2. Questions and comments from the instructor/TA on the whole presentation.
  3. Each member individually describes the part they worked on most and answers further questions about it.
- When helping the team prepare for the defense, split preparation along these three steps and make sure each member has their own "part of the story" ready, not just a general overview.

## 9. Grading criteria

The final grade takes into account:

- Software characteristics: topic, scope, complexity, quality.
- Success in applying the required (agile) development methodologies.
- Quality of the team's collaborative work.
- Success in applying the required technologies and tools (gRPC, DDD, CQRS, message queues, gateway, security, containerization, testing, etc.).

When the user asks for a readiness assessment or "how are we doing", check status against each of these four points, not just the software's functionality.

## 10. Quick checklist (for a fast team status check)

- [ ] Team has 3–7 members, each responsible for at least 1 microservice
- [ ] Technologies from the course used (or approval obtained for deviation)
- [ ] gRPC / DDD / CQRS / message queue / gateway / security covered
- [ ] SPA client (Angular/React/...)
- [ ] Containerization (Docker/Docker Compose or alternative)
- [ ] Unit tests per microservice
- [ ] Integration/E2E tests for the whole system
- [ ] GitHub Project Board active and up to date
- [ ] Meaningful commits attributed per author
- [ ] Minimum 4 sprints, all members participate in every sprint
- [ ] Topic registered via the survey/form (deadline Dec 25, 2025)
- [ ] Documentation: user guide, setup instructions, documented code, class diagrams
- [ ] Application deployed/published
- [ ] Submitted by September 1, 2026
