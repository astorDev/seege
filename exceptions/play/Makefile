TEST ?= main

run:
	dotnet run

kill:
	kill `lsof -t -i:8402`

yac:
	httpyac send $(TEST).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'

v-use:
	cp Versions/$(V)/Program.cs Program.cs

v-save:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

v-play:
	make v-use
	make play

test:
	dotnet test --filter FullyQualifiedName~Seege.Exceptions.Playground.$(NAME) --logger "console;verbosity=detailed"

load-exceptional:
	k6 run exceptional.js

load-resulted:
	k6 run resulted.js