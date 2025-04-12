package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
)

type Module struct {
	ID     string  `json:"id"`
	X      float64 `json:"x"`
	Y      float64 `json:"y"`
	Width  float64 `json:"width"`
	Height float64 `json:"height"`
}

func generateModules() []Module {
	plotWidth := 40.0
	plotHeight := 24.0
	moduleSize := 8.0
	cols := int(plotWidth / moduleSize)
	rows := int(plotHeight / moduleSize)

	var modules []Module
	id := 1
	for i := 0; i < rows; i++ {
		for j := 0; j < cols; j++ {
			modules = append(modules, Module{
				ID:     "M-" + itoa(id),
				X:      float64(j) * moduleSize,
				Y:      float64(i) * moduleSize,
				Width:  moduleSize,
				Height: moduleSize,
			})
			id++
		}
	}
	return modules
}

func itoa(n int) string {
	return fmt.Sprintf("%d", n)
}

func main() {
	http.Handle("/", http.FileServer(http.Dir(".")))

	http.HandleFunc("/api/modules", func(w http.ResponseWriter, r *http.Request) {
		modules := generateModules()
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(modules)
	})

	log.Println("Serwer działa na http://localhost:8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
